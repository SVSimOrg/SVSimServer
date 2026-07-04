using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.PackDrawTables;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Services;

public sealed class GachaPointService : IGachaPointService
{
    private readonly SVSimDbContext _db;
    private readonly IPackDrawTableRepository _drawTables;

    public GachaPointService(SVSimDbContext db, IPackDrawTableRepository drawTables)
    {
        _db = db;
        _drawTables = drawTables;
    }

    public async Task<IReadOnlyList<GachaPointRewardDto>> GetRewardsAsync(int packId, long viewerId)
    {
        var pack = await _db.Packs.FirstOrDefaultAsync(p => p.Id == packId);
        if (pack?.GachaPointConfig is null) return Array.Empty<GachaPointRewardDto>();

        var drawTable = await _drawTables.GetAsync(packId);
        if (drawTable is null) return Array.Empty<GachaPointRewardDto>();

        // EF Core 8 has no ToHashSetAsync on IQueryable — materialize via ToListAsync then hash.
        var receivedCardIds = (await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.GachaPointReceived)
            .Where(r => r.PackId == packId)
            .Select(r => r.CardId)
            .ToListAsync()).ToHashSet();

        // Cards exchangeable for gacha points: the pack's draw-table pool, excluding alt-art
        // (the foil/alt printing is gated separately). The exchange covers (a) every Legendary
        // and (b) any IsLeader card regardless of tier — UCL pack 16015 has Kyoka and Miyako
        // as Gold-tier leaders that prod still offers. Filtering on Legendary alone would miss
        // them. Verified against the captured 16015 response in
        // traffic_prod_all_gacha_exchange.ndjson.
        var exchangeableCardIds = drawTable.CardWeights
            .Where(w => !w.IsAltArt && (w.Tier == DrawTier.Legendary || w.IsLeader))
            .Select(w => w.CardId)
            .ToHashSet();

        // Re-query with Class loaded — pool provider doesn't include navs, so card.Class is
        // null on every pool entry and class_id would collapse to "0".
        var legendariesWithClass = await _db.Cards
            .Where(c => exchangeableCardIds.Contains(c.Id))
            .Include(c => c.Class)
            .ToListAsync();

        // Pull both cosmetic types in one trip. Group by card_id for O(1) lookup below.
        var cosmeticsByCard = await _db.CardCosmeticRewards
            .Where(r => exchangeableCardIds.Contains(r.CardId)
                        && (r.Type == CosmeticType.Emblem || r.Type == CosmeticType.Skin))
            .ToListAsync();
        var cosmeticLookup = cosmeticsByCard
            .GroupBy(r => r.CardId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var standard = new List<GachaPointRewardDto>();
        var leader = new List<GachaPointRewardDto>();

        foreach (var card in legendariesWithClass
            // Neutral cards have Class=null; client wire-encodes them as class_id="0".
            .OrderBy(c => c.Class?.Id ?? 0).ThenBy(c => c.Id))
        {
            cosmeticLookup.TryGetValue(card.Id, out var cosmetics);
            var emblems = cosmetics?.Where(c => c.Type == CosmeticType.Emblem).ToList()
                ?? new List<CardCosmeticReward>();
            var skin = cosmetics?.FirstOrDefault(c => c.Type == CosmeticType.Skin);

            var classId = (card.Class?.Id ?? 0).ToString(CultureInfo.InvariantCulture);
            var isReceived = receivedCardIds.Contains(card.Id);

            if (IsLeaderCard(skin))
            {
                // Leader card — 2 or 3 entries: Sleeve/Card-cosmetic (type 6) with detail=card_id,
                // Skin (type 10) with detail=leader_skin_id, and an Emblem (type 7) per emblem row.
                // Most leader cards in captured packs have exactly 1 emblem, but we emit per-emblem
                // for consistency with the standard-legendary branch.
                var rewardList = new List<GachaPointRewardDetailEntry>
                {
                    new GachaPointRewardDetailEntry
                    {
                        RewardType = (int)UserGoodsType.Sleeve, RewardDetailId = card.Id, RewardNumber = 1,
                    },
                    new GachaPointRewardDetailEntry
                    {
                        RewardType = (int)UserGoodsType.Skin,
                        RewardDetailId = skin!.CosmeticId, RewardNumber = 1,
                    },
                };
                foreach (var emblem in emblems)
                {
                    rewardList.Add(new GachaPointRewardDetailEntry
                    {
                        RewardType = (int)UserGoodsType.Emblem,
                        RewardDetailId = emblem.CosmeticId, RewardNumber = 1,
                    });
                }
                leader.Add(new GachaPointRewardDto
                {
                    ClassId = classId, CardId = card.Id, IsReceived = isReceived,
                    RewardList = rewardList,
                });
            }
            else
            {
                // Standard legendary — one reward_list entry per emblem cosmetic (possibly zero
                // entries for packs whose emblem mappings weren't in the capture sweep, e.g. pack
                // 10001 Classic). The card is still grantable; the exchange's cosmetic cascade
                // delivers whatever rows actually exist in CardCosmeticRewards.
                var dto = new GachaPointRewardDto
                {
                    ClassId = classId, CardId = card.Id, IsReceived = isReceived,
                };
                foreach (var emblem in emblems)
                {
                    dto.RewardList.Add(new GachaPointRewardDetailEntry
                    {
                        RewardType = (int)UserGoodsType.Emblem,
                        RewardDetailId = emblem.CosmeticId,
                        RewardNumber = 1,
                    });
                }
                standard.Add(dto);
            }
        }

        // Standard first, then leader — matches the prod capture order for pack 10008.
        standard.AddRange(leader);
        return standard;
    }

    /// <summary>
    /// Leader cards are identified purely by the data shape: a (non-foil legendary) card with
    /// a <see cref="CosmeticType.Skin"/> cosmetic-reward row is a leader card. There is no
    /// is_leader flag, no card-id pattern, no other signal — the presence of the Skin row is
    /// the entire heuristic. Callers must have already filtered to Rarity.Legendary &amp;&amp;
    /// !IsFoil before invoking this.
    /// </summary>
    private static bool IsLeaderCard(CardCosmeticReward? skin) => skin is not null;

    public void Accrue(Viewer viewer, PackConfigEntry pack, PackChildGachaEntry child, int packNumber)
    {
        if (pack.GachaPointConfig is null) return;
        if (packNumber <= 0) return;

        // Per-child override wins when set (>0); fall back to the pack's default.
        int perPack = child.OverrideIncreaseGachaPoint > 0
            ? child.OverrideIncreaseGachaPoint
            : pack.GachaPointConfig.IncreaseGachaPoint;
        if (perPack <= 0) return;

        int delta = perPack * packNumber;

        var existing = viewer.GachaPointBalances.FirstOrDefault(b => b.PackId == pack.Id);
        if (existing is null)
        {
            viewer.GachaPointBalances.Add(new ViewerGachaPointBalance
            {
                PackId = pack.Id, Points = delta,
            });
        }
        else
        {
            existing.Points += delta;
        }
    }

    public async Task<ExchangeOutcome> TryExchangeAsync(IInventoryTransaction tx, int packId, long cardId)
    {
        var viewer = tx.Viewer;
        var pack = await _db.Packs.FirstOrDefaultAsync(p => p.Id == packId);
        if (pack?.GachaPointConfig is null)
            return ExchangeOutcome.Fail("pack_not_exchangeable");

        int threshold = pack.GachaPointConfig.ExchangeablePoint;
        var balance = viewer.GachaPointBalances.FirstOrDefault(b => b.PackId == packId);
        if (balance is null || balance.Points < threshold)
            return ExchangeOutcome.Fail("insufficient_gacha_points");

        // Validate the card is in the catalog by re-running GetRewardsAsync. This re-uses the
        // same eligibility rules (in-pool + Legendary + has Emblem cosmetic) without
        // duplicating them — and naturally excludes ticket-only packs whose pool we already
        // hide from /pack/info.
        var catalog = await GetRewardsAsync(packId, viewer.Id);
        var entry = catalog.FirstOrDefault(e => e.CardId == cardId);
        if (entry is null)
            return ExchangeOutcome.Fail("card_not_exchangeable");

        if (viewer.GachaPointReceived.Any(r => r.PackId == packId && r.CardId == cardId))
            return ExchangeOutcome.Fail("already_received");

        // Debit balance + mark received. (`balance` is non-null past the earlier guard.)
        balance.Points -= threshold;
        viewer.GachaPointReceived.Add(new ViewerGachaPointReceived
        {
            PackId = packId, CardId = cardId, ReceivedAt = DateTime.UtcNow,
        });

        // Grant the card via the inventory tx — its CardCosmeticReward cascade covers the
        // Emblem (standard legendary) or Skin+Emblem (leader). Convert at the wire boundary
        // so ExchangeOutcome still carries RewardListEntry for the controller response.
        var granted = await tx.GrantAsync(UserGoodsType.Card, cardId, 1);
        var rewardList = granted.ToRewardList();

        return ExchangeOutcome.Ok(rewardList);
    }
}
