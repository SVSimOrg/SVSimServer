using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of battle-pass level rows from <c>seeds/battle-pass-levels.json</c>.
/// Curve is global; rows missing from the seed are LEFT INTACT.
/// </summary>
public class BattlePassImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<BattlePassLevelSeed>(Path.Combine(seedDir, "battle-pass-levels.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[BattlePassImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.BattlePassLevels.ToDictionaryAsync(e => e.Level);
        int created = 0, updated = 0;
        foreach (var s in seed)
        {
            if (s.Level == 0) continue;
            var entry = existing.TryGetValue(s.Level, out var ex) ? ex : new BattlePassLevelEntry { Level = s.Level };
            entry.RequiredPoint = s.RequiredPoint;
            if (ex is null) { context.BattlePassLevels.Add(entry); existing[s.Level] = entry; created++; }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[BattlePassImporter] +{created}/~{updated}");
        return created + updated;
    }
}
