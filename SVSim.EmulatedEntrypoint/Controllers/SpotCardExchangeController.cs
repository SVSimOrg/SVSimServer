using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.SpotCardExchange;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.SpotCardExchange;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /spot_card_exchange/* — trade spot points for individual cards from the rotating exchange
/// pool. Spot points are earned from battles/missions (not implemented here — earners live in
/// battle/mission finish reward emitters via <see cref="UserGoodsType.SpotCardPoint"/>).
/// </summary>
[Route("spot_card_exchange")]
public class SpotCardExchangeController : SVSimController
{
    /// <summary>
    /// Pre-release exchange cap. Captures show "2" — global limit, not per-card. When
    /// IsPreRelease is active on the catalog level we honour this; otherwise the cap is
    /// effectively unbounded (UI never shows the warning).
    /// </summary>
    private const int PreReleaseLimit = 2;

    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly TimeProvider _time;

    public SpotCardExchangeController(SVSimDbContext db, IInventoryService inv, TimeProvider time)
    {
        _db = db;
        _inv = inv;
        _time = time;
    }

    [HttpPost("top")]
    public async Task<ActionResult<SpotCardExchangeTopResponse>> Top(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .Select(v => new { v.Currency.SpotPoints })
            .FirstOrDefaultAsync();
        if (viewer is null) return Unauthorized();

        var catalog = await _db.SpotCardExchangeCatalog
            .Where(c => c.IsEnabled)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var exchanges = await _db.ViewerSpotCardExchanges
            .Where(e => e.ViewerId == viewerId)
            .ToListAsync();
        var exchangedIds = exchanges.Select(e => e.CardId).ToHashSet();
        int preReleaseExchangedCount = exchanges.Count(e => e.IsPreRelease);
        bool preReleaseActive = catalog.Any(c => c.IsPreRelease);
        bool preReleaseLimitHit = preReleaseExchangedCount >= PreReleaseLimit;

        // Build the 9-clan-bucket dict-of-arrays. Every clan slot is present even when empty;
        // the inner dict always uses key "1" matching the captured prod shape.
        var byClan = new List<Dictionary<string, List<SpotCardExchangeCardDto>>>(9);
        for (int clan = 0; clan < 9; clan++)
        {
            byClan.Add(new Dictionary<string, List<SpotCardExchangeCardDto>>
            {
                ["1"] = new List<SpotCardExchangeCardDto>(),
            });
        }

        foreach (var c in catalog)
        {
            int clanIdx = Math.Clamp(c.ClassId, 0, 8);
            byClan[clanIdx]["1"].Add(new SpotCardExchangeCardDto
            {
                CardId = c.Id,
                ExchangeStatus = ComputeExchangeStatus(c, exchangedIds, preReleaseLimitHit),
                ExchangePoint = c.ExchangePoint.ToString(),
                Class = c.ClassId.ToString(),
                IsPreRelease = c.IsPreRelease,
                TsRotationId = c.TsRotationId.ToString(),
            });
        }

        return new SpotCardExchangeTopResponse
        {
            SpotPoint = checked((int)viewer.SpotPoints),
            ExchangeableCardList = byClan,
            SoonCycleOutCardSetId = string.Empty,   // No captured value to derive; spec allows ""
            PreReleaseInfo = new PreReleaseInfoDto
            {
                IsPreRelease = preReleaseActive,
                PreReleaseSpotCardExchangeCount = preReleaseExchangedCount,
                PreReleaseSpotCardExchangeLimit = PreReleaseLimit,
            },
        };
    }

    [HttpPost("exchange")]
    public async Task<ActionResult<SpotCardExchangeResponse>> Exchange(SpotCardExchangeRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var entry = await _db.SpotCardExchangeCatalog.FindAsync((long)request.CardId);
        if (entry is null || !entry.IsEnabled)
            return BadRequest(new { error = "unknown_card" });

        // Already-exchanged guard — each catalog row is one card per viewer.
        var existingExchange = await _db.ViewerSpotCardExchanges
            .FirstOrDefaultAsync(e => e.ViewerId == viewerId && e.CardId == entry.Id);
        if (existingExchange is not null)
            return BadRequest(new { error = "already_exchanged" });

        if (entry.IsPreRelease)
        {
            int prCount = await _db.ViewerSpotCardExchanges
                .CountAsync(e => e.ViewerId == viewerId && e.IsPreRelease);
            if (prCount >= PreReleaseLimit)
                return BadRequest(new { error = "pre_release_limit_reached" });
        }

        await using var tx = await _inv.BeginAsync(viewerId, configure: cfg => cfg.Source = GrantSource.GachaPointExchange);

        var rewardList = new List<RewardListEntry>();

        // Debit spot points. Client-supplied exchange_point isn't authoritative — server uses
        // catalog price. Mirroring the build_deck/sleeve convention: post-state currency entry
        // first, then grants.
        var spotRes = await tx.TrySpendAsync(SpendCurrency.SpotPoint, entry.ExchangePoint);
        if (!spotRes.Success)
            return BadRequest(new { error = "insufficient_spot_points" });
        rewardList.Add(new RewardListEntry
        {
            RewardType = (int)UserGoodsType.SpotCardPoint,
            RewardId = 0,
            RewardNum = checked((int)spotRes.PostStateTotal),
        });

        // Grant the card itself via the inventory tx (handles cosmetic cascade).
        var granted = await tx.GrantAsync(UserGoodsType.Card, entry.Id, 1);
        rewardList.AddRange(granted.ToRewardList());

        _db.ViewerSpotCardExchanges.Add(new ViewerSpotCardExchange
        {
            ViewerId = viewerId,
            CardId = entry.Id,
            IsPreRelease = entry.IsPreRelease,
            ExchangedAt = _time.GetUtcNow().UtcDateTime,
        });

        await tx.CommitAsync();
        return new SpotCardExchangeResponse { RewardList = rewardList };
    }

    /// <summary>
    /// Maps to <see cref="Wizard.SpotCardExchangeInfo.ExchangeStatus"/>:
    ///   0 = EnableExchange
    ///   1 = AlreadyExchange (viewer has already exchanged this card)
    ///   2 = LimitOver (pre-release card and viewer hit the global pre-release cap)
    /// Insufficient-balance is NOT surfaced here — the client greys those out by comparing
    /// <c>spot_point</c> to <c>exchange_point</c>.
    /// </summary>
    private static int ComputeExchangeStatus(SpotCardExchangeEntry c, HashSet<long> exchangedIds, bool preReleaseLimitHit)
    {
        if (exchangedIds.Contains(c.Id)) return 1;
        if (c.IsPreRelease && preReleaseLimitHit) return 2;
        return 0;
    }

}
