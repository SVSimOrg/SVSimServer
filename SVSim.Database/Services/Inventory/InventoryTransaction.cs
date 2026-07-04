using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;

namespace SVSim.Database.Services.Inventory;

internal sealed class InventoryTransaction : IInventoryTransaction
{
    private const int AcquireHistoryRetention = InventoryHistoryConfig.RetentionRowsPerViewer;

    private readonly SVSimDbContext _db;
    private readonly IDbContextTransaction _dbTx;
    private readonly ILogger _log;
    private readonly FreeplayConfig _freeplay;
    private readonly GrantSource _source;
    private bool _committed;

    public Viewer Viewer { get; }
    public bool IsFreeplay => _freeplay.Enabled;

    private readonly List<InventoryOp> _ops = new();

    internal abstract record InventoryOp;
    internal sealed record SpendOp(SpendCurrency Currency, long Cost, long PostState) : InventoryOp;
    internal sealed record GrantOp(UserGoodsType Type, long DetailId, int Num, int PostStateOrCount, bool IsCascade) : InventoryOp;

    public InventoryTransaction(
        SVSimDbContext db,
        IDbContextTransaction dbTx,
        Viewer viewer,
        FreeplayConfig freeplay,
        GrantSource source,
        ILogger log)
    {
        _db = db;
        _dbTx = dbTx;
        Viewer = viewer;
        _freeplay = freeplay;
        _source = source;
        _log = log;
    }

    public Task<SpendResult> TrySpendAsync(SpendCurrency currency, long cost, CancellationToken ct = default)
    {
        ThrowIfCommitted();
        if (cost < 0) cost = 0;

        if (_freeplay.Enabled && currency != SpendCurrency.SpotPoint)
        {
            long amount = checked((long)_freeplay.CurrencyAmount);
            _ops.Add(new SpendOp(currency, cost, amount));
            return Task.FromResult(new SpendResult(SpendOutcome.Success, amount));
        }

        ulong current = ReadBalance(currency);
        if (current < (ulong)cost)
            return Task.FromResult(new SpendResult(SpendOutcome.Insufficient, (long)current));

        ulong post = current - (ulong)cost;
        WriteBalance(currency, post);
        _ops.Add(new SpendOp(currency, cost, (long)post));
        return Task.FromResult(new SpendResult(SpendOutcome.Success, (long)post));
    }

    private ulong ReadBalance(SpendCurrency c) => c switch
    {
        SpendCurrency.Crystal => Viewer.Currency.Crystals,
        SpendCurrency.Rupee => Viewer.Currency.Rupees,
        SpendCurrency.RedEther => Viewer.Currency.RedEther,
        SpendCurrency.SpotPoint => Viewer.Currency.SpotPoints,
        _ => throw new ArgumentOutOfRangeException(nameof(c)),
    };

    private void WriteBalance(SpendCurrency c, ulong value)
    {
        switch (c)
        {
            case SpendCurrency.Crystal: Viewer.Currency.Crystals = value; break;
            case SpendCurrency.Rupee: Viewer.Currency.Rupees = value; break;
            case SpendCurrency.RedEther: Viewer.Currency.RedEther = value; break;
            case SpendCurrency.SpotPoint: Viewer.Currency.SpotPoints = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(c));
        }
    }

    public Task<SpendResult> TryDebitAsync(UserGoodsType type, long detailId, int num, CancellationToken ct = default)
    {
        ThrowIfCommitted();
        return type switch
        {
            UserGoodsType.Crystal => TrySpendAsync(SpendCurrency.Crystal, num, ct),
            UserGoodsType.Rupy => TrySpendAsync(SpendCurrency.Rupee, num, ct),
            UserGoodsType.RedEther => TrySpendAsync(SpendCurrency.RedEther, num, ct),
            UserGoodsType.SpotCardPoint => TrySpendAsync(SpendCurrency.SpotPoint, num, ct),
            UserGoodsType.Item => Task.FromResult(DebitItem(detailId, num)),
            _ => throw new NotSupportedException($"Debit not supported for {type}"),
        };
    }

    private SpendResult DebitItem(long detailId, int num)
    {
        var owned = Viewer.Items.FirstOrDefault(i => i.Item.Id == (int)detailId);
        if (owned is null)
            throw new InventoryCatalogException($"Item {detailId} not owned by viewer");
        if (owned.Count < num)
            return new SpendResult(SpendOutcome.Insufficient, owned.Count);
        owned.Count -= num;
        // Item debit logged as a synthetic SpendOp so CommitAsync can track it.
        // Sentinel currency (int)-1 is filtered out by CommitAsync's currency-collision loop.
        _ops.Add(new SpendOp((SpendCurrency)(-1) /* sentinel */, num, owned.Count));
        // IsCascade: true so this GrantOp is excluded from BuildDeltas output.
        _ops.Add(new GrantOp(UserGoodsType.Item, detailId, 0, owned.Count, IsCascade: true));
        return new SpendResult(SpendOutcome.Success, owned.Count);
    }

    public async Task<IReadOnlyList<GrantedReward>> GrantAsync(UserGoodsType type, long detailId, int num, CancellationToken ct = default)
    {
        ThrowIfCommitted();

        switch (type)
        {
            case UserGoodsType.Rupy:
                Viewer.Currency.Rupees += (ulong)num;
                var rupy = checked((int)Viewer.Currency.Rupees);
                _ops.Add(new GrantOp(type, detailId, num, rupy, false));
                return Single(type, detailId, rupy);

            case UserGoodsType.Crystal:
                Viewer.Currency.Crystals += (ulong)num;
                var crystal = checked((int)Viewer.Currency.Crystals);
                _ops.Add(new GrantOp(type, detailId, num, crystal, false));
                return Single(type, detailId, crystal);

            case UserGoodsType.RedEther:
                Viewer.Currency.RedEther += (ulong)num;
                var red = checked((int)Viewer.Currency.RedEther);
                _ops.Add(new GrantOp(type, detailId, num, red, false));
                return Single(type, detailId, red);

            case UserGoodsType.SpotCardPoint:
                Viewer.Currency.SpotPoints += (ulong)num;
                var spot = checked((int)Viewer.Currency.SpotPoints);
                _ops.Add(new GrantOp(type, detailId, num, spot, false));
                return Single(type, detailId, spot);

            case UserGoodsType.Sleeve:
                AddCosmeticIfMissing(Viewer.Sleeves, detailId, _db.Sleeves);
                _ops.Add(new GrantOp(type, detailId, num, 1, false));
                return Single(type, detailId, 1);

            case UserGoodsType.Emblem:
                AddCosmeticIfMissing(Viewer.Emblems, detailId, _db.Emblems);
                _ops.Add(new GrantOp(type, detailId, num, 1, false));
                return Single(type, detailId, 1);

            case UserGoodsType.Skin:
                AddCosmeticIfMissing(Viewer.LeaderSkins, detailId, _db.LeaderSkins);
                _ops.Add(new GrantOp(type, detailId, num, 1, false));
                return Single(type, detailId, 1);

            case UserGoodsType.Degree:
                AddCosmeticIfMissing(Viewer.Degrees, detailId, _db.Degrees);
                _ops.Add(new GrantOp(type, detailId, num, 1, false));
                return Single(type, detailId, 1);

            case UserGoodsType.MyPageBG:
                AddCosmeticIfMissing(Viewer.MyPageBackgrounds, detailId, _db.MyPageBackgrounds);
                _ops.Add(new GrantOp(type, detailId, num, 1, false));
                return Single(type, detailId, 1);

            case UserGoodsType.Item:
            {
                var owned = Viewer.Items.FirstOrDefault(i => i.Item.Id == (int)detailId);
                int post;
                if (owned is null)
                {
                    var item = _db.Items.Find((int)detailId)
                               ?? throw new InventoryCatalogException($"Item {detailId} not in catalog");
                    Viewer.Items.Add(new OwnedItemEntry { Item = item, Count = num, Viewer = Viewer });
                    post = num;
                }
                else
                {
                    owned.Count += num;
                    post = owned.Count;
                }
                _ops.Add(new GrantOp(type, detailId, num, post, false));
                return Single(type, detailId, post);
            }

            case UserGoodsType.Card:
                return await ApplyCardAsync(detailId, num, ct);

            case UserGoodsType.SpotCard:
            case UserGoodsType.SpotCardOnlyLatestCardPack:
                throw new NotSupportedException(
                    $"{type} rewards are not yet supported — emitters use Card=5 instead.");

            default:
                throw new NotImplementedException(
                    $"UserGoodsType {type} grant lands in a subsequent task");
        }
    }

    public async Task<int> BackfillCardCosmeticsAsync(CancellationToken ct = default)
    {
        ThrowIfCommitted();

        var lookupIds = Viewer.Cards
            .Select(c => c.Card.IsFoil ? c.Card.Id - 1 : c.Card.Id)
            .Distinct()
            .ToList();

        var cascade = await _db.CardCosmeticRewards
            .Where(r => lookupIds.Contains(r.CardId))
            .ToListAsync(ct);

        int granted = 0;
        foreach (var reward in cascade)
        {
            if (AlreadyOwnsCosmetic(reward.Type, reward.CosmeticId)) continue;
            if (TryAddCascadeCosmetic(reward, reward.CardId))
            {
                granted++;
                _ops.Add(new GrantOp((UserGoodsType)(int)reward.Type, reward.CosmeticId, 1, 1, true));
            }
        }

        return granted;
    }

    private bool AlreadyOwnsCosmetic(CosmeticType type, long id) => type switch
    {
        CosmeticType.Sleeve   => Viewer.Sleeves.Any(s => s.Id == id),
        CosmeticType.Emblem   => Viewer.Emblems.Any(e => e.Id == id),
        CosmeticType.Skin     => Viewer.LeaderSkins.Any(s => s.Id == id),
        CosmeticType.Degree   => Viewer.Degrees.Any(d => d.Id == id),
        CosmeticType.MyPageBG => Viewer.MyPageBackgrounds.Any(b => b.Id == id),
        _ => false,
    };

    public long EffectiveBalance(SpendCurrency currency)
    {
        if (_freeplay.Enabled && currency != SpendCurrency.SpotPoint)
            return checked((long)_freeplay.CurrencyAmount);

        return currency switch
        {
            SpendCurrency.Crystal => (long)Viewer.Currency.Crystals,
            SpendCurrency.Rupee => (long)Viewer.Currency.Rupees,
            SpendCurrency.RedEther => (long)Viewer.Currency.RedEther,
            SpendCurrency.SpotPoint => (long)Viewer.Currency.SpotPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(currency)),
        };
    }

    public bool OwnsCard(long cardId)
        => _freeplay.Enabled || Viewer.Cards.Any(c => c.Card.Id == cardId && c.Count > 0);

    public bool OwnsCosmetic(CosmeticType type, int id)
    {
        if (_freeplay.Enabled) return true;
        return type switch
        {
            CosmeticType.Sleeve => Viewer.Sleeves.Any(s => s.Id == id),
            CosmeticType.Emblem => Viewer.Emblems.Any(e => e.Id == id),
            CosmeticType.Degree => Viewer.Degrees.Any(d => d.Id == id),
            CosmeticType.Skin => Viewer.LeaderSkins.Any(s => s.Id == id),
            CosmeticType.MyPageBG => Viewer.MyPageBackgrounds.Any(m => m.Id == id),
            _ => false,
        };
    }

    public async Task<InventoryCommitResult> CommitAsync(CancellationToken ct = default)
    {
        ThrowIfCommitted();

        // Flush entity mutations first so audit-history rows are staged on top of post-commit state.
        await _db.SaveChangesAsync(ct);

        WriteAcquireHistory();
        await _db.SaveChangesAsync(ct);

        await PruneAcquireHistoryAsync(ct);

        await _dbTx.CommitAsync(ct);
        _committed = true;

        var rewardList = BuildRewardList();
        var deltas = BuildDeltas();
        return new InventoryCommitResult(rewardList, deltas);
    }

    private async Task PruneAcquireHistoryAsync(CancellationToken ct)
    {
        // Two-phase: SQLite (used in tests) cannot translate Skip+OrderBy inside ExecuteDeleteAsync.
        var overflowIds = await _db.ViewerAcquireHistory
            .Where(h => h.ViewerId == Viewer.Id)
            .OrderByDescending(h => h.AcquireTime).ThenByDescending(h => h.Id)
            .Skip(AcquireHistoryRetention)
            .Select(h => h.Id)
            .ToListAsync(ct);

        if (overflowIds.Count == 0) return;

        await _db.ViewerAcquireHistory
            .Where(h => overflowIds.Contains(h.Id))
            .ExecuteDeleteAsync(ct);
    }

    private void WriteAcquireHistory()
    {
        var now = DateTime.UtcNow;
        var primaryMessage = GrantSourceMessages.For(_source);
        var cascadeMessage = GrantSourceMessages.For(GrantSource.CardCosmeticCascade);

        foreach (var op in _ops)
        {
            if (op is not GrantOp grant) continue;
            if (grant.Num == 0) continue;  // skip synthetic post-state grants (e.g. DebitItem)

            var rowSource = grant.IsCascade ? GrantSource.CardCosmeticCascade : _source;
            var rowMessage = grant.IsCascade ? cascadeMessage : primaryMessage;
            var detailId = IsCurrency(grant.Type) ? 0L : grant.DetailId;

            _db.ViewerAcquireHistory.Add(new ViewerAcquireHistoryEntry
            {
                ViewerId = Viewer.Id,
                RewardType = (int)grant.Type,
                RewardDetailId = detailId,
                RewardCount = grant.Num,
                AcquireType = (int)rowSource,
                Message = rowMessage,
                AcquireTime = now,
            });
        }
    }

    private IReadOnlyList<GrantedReward> BuildRewardList()
    {
        // Pass 1 — for each currency type, find the last op (spend OR grant) that touched it
        // and emit a single entry with its post-state. Skip the sentinel item-debit currency.
        var lastCurrencyPost = new Dictionary<UserGoodsType, int>();
        var orderedTouches = new List<UserGoodsType>(); // preserve first-touch order for stable output

        foreach (var op in _ops)
        {
            switch (op)
            {
                case SpendOp s when (int)s.Currency >= 0:
                    var goodsForSpend = SpendCurrencyToGoodsType(s.Currency);
                    if (!lastCurrencyPost.ContainsKey(goodsForSpend)) orderedTouches.Add(goodsForSpend);
                    lastCurrencyPost[goodsForSpend] = checked((int)s.PostState);
                    break;

                case GrantOp g when IsCurrency(g.Type):
                    if (!lastCurrencyPost.ContainsKey(g.Type)) orderedTouches.Add(g.Type);
                    lastCurrencyPost[g.Type] = g.PostStateOrCount;
                    break;
            }
        }

        var output = new List<GrantedReward>();
        foreach (var type in orderedTouches)
        {
            output.Add(new GrantedReward(type, 0, lastCurrencyPost[type]));
        }

        // Pass 2 — non-currency grants: one entry per (type, id) using LAST post-state for items
        // and cards (collapses multi-add to final count) and 1 for cosmetics.
        var nonCurrencyKey = new Dictionary<(UserGoodsType, long), int>();
        var nonCurrencyOrder = new List<(UserGoodsType, long)>();

        foreach (var op in _ops.OfType<GrantOp>())
        {
            if (IsCurrency(op.Type)) continue;
            var key = (op.Type, op.DetailId);
            if (!nonCurrencyKey.ContainsKey(key)) nonCurrencyOrder.Add(key);
            nonCurrencyKey[key] = op.PostStateOrCount;
        }
        foreach (var (type, id) in nonCurrencyOrder)
        {
            output.Add(new GrantedReward(type, id, nonCurrencyKey[(type, id)]));
        }
        return output;
    }

    private IReadOnlyList<GrantedReward> BuildDeltas()
        => _ops.OfType<GrantOp>()
            .Where(o => !o.IsCascade)
            .Select(o => new GrantedReward(o.Type, o.DetailId, o.Num))
            .ToList();

    private static bool IsCurrency(UserGoodsType t) =>
        t is UserGoodsType.Crystal
           or UserGoodsType.Rupy
           or UserGoodsType.RedEther
           or UserGoodsType.SpotCardPoint;

    private static UserGoodsType SpendCurrencyToGoodsType(SpendCurrency c) => c switch
    {
        SpendCurrency.Crystal => UserGoodsType.Crystal,
        SpendCurrency.Rupee => UserGoodsType.Rupy,
        SpendCurrency.RedEther => UserGoodsType.RedEther,
        SpendCurrency.SpotPoint => UserGoodsType.SpotCardPoint,
        _ => throw new ArgumentOutOfRangeException(nameof(c)),
    };

    private static IReadOnlyList<GrantedReward> Single(UserGoodsType type, long id, int num)
        => new[] { new GrantedReward(type, id, num) };

    private void ThrowIfCommitted()
    {
        if (_committed)
            throw new InvalidOperationException("Inventory transaction already committed");
    }

    private async Task<IReadOnlyList<GrantedReward>> ApplyCardAsync(long cardId, int num, CancellationToken ct)
    {
        var owned = Viewer.Cards.FirstOrDefault(c => c.Card.Id == cardId);
        int postCount;
        if (owned is null)
        {
            var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == cardId, ct)
                       ?? throw new InventoryCatalogException($"Card {cardId} not in catalog");
            owned = new OwnedCardEntry { Card = card, Count = num, IsProtected = false };
            Viewer.Cards.Add(owned);
            postCount = num;
        }
        else
        {
            owned.Count += num;
            postCount = owned.Count;
        }

        var results = new List<GrantedReward>
        {
            new(UserGoodsType.Card, cardId, postCount),
        };
        _ops.Add(new GrantOp(UserGoodsType.Card, cardId, num, postCount, false));

        long lookupId = owned.Card.IsFoil ? cardId - 1 : cardId;
        var cascade = await _db.CardCosmeticRewards
            .Where(r => r.CardId == lookupId)
            .ToListAsync(ct);

        foreach (var reward in cascade)
        {
            if (TryAddCascadeCosmetic(reward, lookupId))
            {
                results.Add(new GrantedReward((UserGoodsType)reward.Type, reward.CosmeticId, 1));
                _ops.Add(new GrantOp((UserGoodsType)reward.Type, reward.CosmeticId, 1, 1, true));
            }
        }

        return results;
    }

    private bool TryAddCascadeCosmetic(CardCosmeticReward reward, long forCardId)
    {
        try
        {
            return reward.Type switch
            {
                CosmeticType.Sleeve   => AddCosmeticIfMissing(Viewer.Sleeves,           reward.CosmeticId, _db.Sleeves),
                CosmeticType.Emblem   => AddCosmeticIfMissing(Viewer.Emblems,           reward.CosmeticId, _db.Emblems),
                CosmeticType.Skin     => AddCosmeticIfMissing(Viewer.LeaderSkins,       reward.CosmeticId, _db.LeaderSkins),
                CosmeticType.Degree   => AddCosmeticIfMissing(Viewer.Degrees,           reward.CosmeticId, _db.Degrees),
                CosmeticType.MyPageBG => AddCosmeticIfMissing(Viewer.MyPageBackgrounds, reward.CosmeticId, _db.MyPageBackgrounds),
                _ => false,
            };
        }
        catch (InventoryCatalogException ex)
        {
            _log.LogWarning(ex,
                "Card cascade: cosmetic {Type} {Id} for card {CardId} skipped (master row missing)",
                reward.Type, reward.CosmeticId, forCardId);
            return false;
        }
    }

    private static bool AddCosmeticIfMissing<T>(List<T> collection, long detailId, Microsoft.EntityFrameworkCore.DbSet<T> catalog) where T : class
    {
        if (collection.Any(e => GetId(e) == detailId)) return false;
        var entity = catalog.Find(checked((int)detailId))
                     ?? throw new InventoryCatalogException(
                         $"Cosmetic id {detailId} not in catalog for type {typeof(T).Name}");
        collection.Add(entity);
        return true;
    }

    private static long GetId<T>(T e)
    {
        var prop = typeof(T).GetProperty("Id")
                   ?? throw new InvalidOperationException($"Type {typeof(T).Name} missing Id property");
        var val = prop.GetValue(e);
        return val switch { long l => l, int i => i, _ => 0 };
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await _dbTx.RollbackAsync();
            _db.ChangeTracker.Clear();
        }
        await _dbTx.DisposeAsync();
    }
}
