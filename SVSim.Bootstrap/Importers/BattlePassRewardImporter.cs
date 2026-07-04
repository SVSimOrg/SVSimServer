using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Authoritative upsert of battle-pass rewards from <c>seeds/battle-pass-rewards.json</c>.
/// For each (season_id, track, level) row in the seed: upsert. For rows in the DB that match
/// a seed-mentioned season but are NOT in the seed: DELETE (seed is authoritative per season).
/// Rewards for seasons not mentioned in the seed are left untouched.
/// </summary>
public class BattlePassRewardImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<BattlePassRewardSeed>(Path.Combine(seedDir, "battle-pass-rewards.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[BattlePassRewardImporter] No seed rows; skipping.");
            return 0;
        }

        var seededSeasonIds = seed.Select(s => s.SeasonId).Distinct().ToHashSet();
        var dbRows = await context.BattlePassRewards
            .Where(r => seededSeasonIds.Contains(r.SeasonId))
            .ToListAsync();
        var dbByKey = dbRows.ToDictionary(r => (r.SeasonId, r.Track, r.Level));

        int created = 0, updated = 0;
        var seenKeys = new HashSet<(int, BattlePassTrack, int)>();
        foreach (var s in seed)
        {
            var track = ParseTrack(s.Track);
            var key = (s.SeasonId, track, s.Level);
            seenKeys.Add(key);
            if (dbByKey.TryGetValue(key, out var ex))
            {
                ex.RewardType = (UserGoodsType)s.RewardType;
                ex.RewardDetailId = s.RewardDetailId;
                ex.RewardNumber = s.RewardNumber;
                ex.IsAppealExclusion = s.IsAppealExclusion;
                updated++;
            }
            else
            {
                context.BattlePassRewards.Add(new BattlePassRewardEntry
                {
                    Id = MakeId(s.SeasonId, track, s.Level),
                    SeasonId = s.SeasonId, Track = track, Level = s.Level,
                    RewardType = (UserGoodsType)s.RewardType, RewardDetailId = s.RewardDetailId,
                    RewardNumber = s.RewardNumber, IsAppealExclusion = s.IsAppealExclusion,
                });
                created++;
            }
        }

        // Authoritative deletion within seeded seasons.
        int deleted = 0;
        foreach (var row in dbRows)
        {
            if (!seenKeys.Contains((row.SeasonId, row.Track, row.Level)))
            {
                context.BattlePassRewards.Remove(row);
                deleted++;
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[BattlePassRewardImporter] +{created}/~{updated}/-{deleted}");
        return created + updated;
    }

    private static BattlePassTrack ParseTrack(string s) => s.ToLowerInvariant() switch
    {
        "normal" => BattlePassTrack.Normal,
        "premium" => BattlePassTrack.Premium,
        _ => throw new InvalidOperationException($"unknown battle pass track: {s}"),
    };

    /// <summary>
    /// Derives a stable surrogate PK from the (SeasonId, Track, Level) natural key.
    /// Encoding: season * 10_000 + track * 1_000 + level.
    /// Safe for season &lt; 10_000, track ∈ {0,1}, level &lt; 1_000 — all realistic values.
    /// </summary>
    private static long MakeId(int seasonId, BattlePassTrack track, int level) =>
        (long)seasonId * 10_000L + (int)track * 1_000 + level;
}
