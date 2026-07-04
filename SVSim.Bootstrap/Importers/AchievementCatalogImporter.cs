using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of achievement-catalog rows from <c>seeds/achievement-catalog.json</c>.
/// Keyed by (AchievementType, Level) so re-running with new captures grows the ladder.
/// Rows missing from the seed are LEFT INTACT.
/// </summary>
public class AchievementCatalogImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<AchievementCatalogSeed>(Path.Combine(seedDir, "achievement-catalog.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[AchievementCatalogImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.AchievementCatalog
            .ToDictionaryAsync(e => (e.AchievementType, e.Level));
        // Fail fast on drift between seed event_type values and the code-side registry.
        // Missing prefixes silently produce counters no emitter ever writes to; typos likewise.
        var unregistered = seed
            .Where(s => s.EventType is not null && !MissionEventKeys.IsRegistered(s.EventType))
            .Select(s => s.EventType!)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (unregistered.Count > 0)
        {
            throw new InvalidOperationException(
                "[AchievementCatalogImporter] seed rows reference unregistered event_type(s): "
                + string.Join(", ", unregistered)
                + ". Add the top-level prefix to MissionEventKeys.RegisteredPrefixes, or fix the seed.");
        }

        int created = 0, updated = 0;
        var unmappedTypes = new HashSet<int>();
        foreach (var s in seed)
        {
            if (s.AchievementType == 0 || s.Level == 0) continue;
            var key = (s.AchievementType, s.Level);
            var entry = existing.TryGetValue(key, out var ex) ? ex : new AchievementCatalogEntry
            {
                AchievementType = s.AchievementType,
                Level = s.Level,
            };
            entry.Name = s.Name;
            entry.RequireNumber = s.RequireNumber;
            entry.RewardType = (UserGoodsType)s.RewardType;
            entry.RewardDetailId = s.RewardDetailId;
            entry.RewardNumber = s.RewardNumber;
            entry.OrderNum = s.OrderNum;
            entry.EventType = s.EventType;
            entry.EventArg = s.EventArg;
            if (ex is null) { context.AchievementCatalog.Add(entry); existing[key] = entry; created++; }
            else updated++;
            if (s.EventType is null) unmappedTypes.Add(s.AchievementType);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[AchievementCatalogImporter] +{created}/~{updated}");
        if (unmappedTypes.Count > 0)
        {
            Console.WriteLine($"[AchievementCatalogImporter] WARN: {unmappedTypes.Count} types " +
                              $"with no event_type: [{string.Join(", ", unmappedTypes.OrderBy(x => x))}] — " +
                              "add to ACHIEVEMENT_EVENT_MAP in data_dumps/scripts/extract-achievements.py");
        }
        return created + updated;
    }
}
