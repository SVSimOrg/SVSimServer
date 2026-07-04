using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

public class ArenaTwoPickCardPoolService : IArenaTwoPickCardPoolService
{
    private readonly SVSimDbContext _db;
    private readonly IGameConfigService _config;

    public ArenaTwoPickCardPoolService(SVSimDbContext db, IGameConfigService config)
    {
        _db = db; _config = config;
    }

    public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng) =>
        GeneratePickSetsForTurn(classId, turn, startingPairId, rng, poolCardSetIds: null);

    public List<CandidatePair> GeneratePickSetsForTurn(
        int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds)
    {
        var aCfg = _config.Get<ArenaTwoPickConfig>();

        // Caller-supplied override wins (e.g. ColosseumSeasonConfig.PoolCardSetIds). Falls
        // back to ChallengeConfig → RotationConfig per the original TK2 resolution chain.
        IReadOnlyList<int> setIds;
        if (poolCardSetIds is { Count: > 0 })
        {
            setIds = poolCardSetIds;
        }
        else
        {
            var cCfg = _config.Get<ChallengeConfig>();
            setIds = cCfg.PoolCardSetIds is { Count: > 0 } ids
                ? ids
                : _config.Get<RotationConfig>().RotationCardSetIds ?? new List<int>();
        }

        var setIdsArr = setIds.ToArray();

        // Cards belong to sets via the ShadowverseCardSetEntry.Cards collection.
        // Class membership uses the Class navigation property: null = neutral (classId 0).
        // Collectible = CollectionInfo != null.
        var pool = _db.CardSets
            .Where(s => setIdsArr.Contains(s.Id))
            .SelectMany(s => s.Cards)
            .Include(c => c.Class)
            .Include(c => c.CollectionInfo)
            .Where(c => c.CollectionInfo != null)
            .Where(c => c.Class == null || c.Class.Id == classId)
            .ToList();

        // Group by (isNeutral, Rarity) for O(1) bucket lookup.
        var byBucket = pool
            .GroupBy(c => (c.Class == null, c.Rarity))
            .ToDictionary(g => g.Key, g => g.ToList());

        var pairs = new List<CandidatePair>(2);
        for (int setNum = 1; setNum <= 2; setNum++)
        {
            pairs.Add(new CandidatePair
            {
                Id = startingPairId + (setNum - 1),
                Turn = turn,
                SetNum = setNum,
                CardId1 = DrawOne(byBucket, aCfg, rng),
                CardId2 = DrawOne(byBucket, aCfg, rng),
                IsSelected = false,
            });
        }
        return pairs;
    }

    private static long DrawOne(
        Dictionary<(bool isNeutral, Rarity rarity), List<ShadowverseCardEntry>> byBucket,
        ArenaTwoPickConfig cfg,
        IRandom rng)
    {
        var rarity = PickRarity(cfg, rng);
        var isNeutral = rng.NextDouble() < cfg.NeutralMixRate;

        var candidates =
            TryBucket(byBucket, isNeutral, rarity)
            ?? TryBucket(byBucket, !isNeutral, rarity)
            ?? FallbackByRarity(byBucket, isNeutral, rarity)
            ?? byBucket.Values.SelectMany(v => v).ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException(
                "ArenaTwoPickCardPoolService: card pool is empty for the configured class/set scope");

        var pick = candidates[rng.Next(candidates.Count)];
        return pick.Id;
    }

    private static List<ShadowverseCardEntry>? TryBucket(
        Dictionary<(bool, Rarity), List<ShadowverseCardEntry>> b,
        bool neutral,
        Rarity r) =>
        b.TryGetValue((neutral, r), out var v) && v.Count > 0 ? v : null;

    private static List<ShadowverseCardEntry>? FallbackByRarity(
        Dictionary<(bool, Rarity), List<ShadowverseCardEntry>> b,
        bool neutral,
        Rarity r)
    {
        Rarity[] order = { Rarity.Legendary, Rarity.Gold, Rarity.Silver, Rarity.Bronze };
        int idx = Array.IndexOf(order, r);
        for (int i = idx + 1; i < order.Length; i++)
        {
            if (TryBucket(b, neutral, order[i]) is { } v)  return v;
            if (TryBucket(b, !neutral, order[i]) is { } w) return w;
        }
        return null;
    }

    private static Rarity PickRarity(ArenaTwoPickConfig cfg, IRandom rng)
    {
        var roll = rng.NextDouble();
        if (roll < cfg.LegendaryRate) return Rarity.Legendary;
        roll -= cfg.LegendaryRate;
        if (roll < cfg.GoldRate)      return Rarity.Gold;
        roll -= cfg.GoldRate;
        if (roll < cfg.SilverRate)    return Rarity.Silver;
        return Rarity.Bronze;
    }
}
