using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of BP monthly mission rows from <c>seeds/bp-monthly-missions.json</c>.
/// Keyed by (Year, Month, OrderNum). Rows missing from the seed are LEFT INTACT.
/// </summary>
public class BattlePassMonthlyMissionImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<BattlePassMonthlyMissionSeed>(
            Path.Combine(seedDir, "bp-monthly-missions.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[BattlePassMonthlyMissionImporter] No seed rows; skipping.");
            return 0;
        }

        // Fail fast on drift between seed event_type values and the code-side registry.
        var unregistered = seed
            .Where(s => s.EventType is not null && !MissionEventKeys.IsRegistered(s.EventType))
            .Select(s => s.EventType!)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (unregistered.Count > 0)
        {
            throw new InvalidOperationException(
                "[BattlePassMonthlyMissionImporter] seed rows reference unregistered event_type(s): "
                + string.Join(", ", unregistered)
                + ". Add the top-level prefix to MissionEventKeys.RegisteredPrefixes, or fix the seed.");
        }

        var existing = await context.BattlePassMonthlyMissions
            .ToDictionaryAsync(e => (e.Year, e.Month, e.OrderNum));
        int created = 0, updated = 0;
        var unmapped = new List<string>();
        foreach (var s in seed)
        {
            if (s.Year == 0 || s.Month == 0) continue;
            var key = (s.Year, s.Month, s.OrderNum);
            var entry = existing.TryGetValue(key, out var ex)
                ? ex
                : new BattlePassMonthlyMissionEntry
                {
                    Year = s.Year, Month = s.Month, OrderNum = s.OrderNum,
                };
            entry.Name = s.Name;
            entry.RequireNumber = s.RequireNumber;
            entry.BattlePassPoint = s.BattlePassPoint;
            entry.RewardType = (UserGoodsType?)s.RewardType;
            entry.RewardDetailId = s.RewardDetailId;
            entry.RewardNumber = s.RewardNumber;
            entry.EventType = s.EventType;
            entry.EventArg = s.EventArg;
            if (ex is null) { context.BattlePassMonthlyMissions.Add(entry); existing[key] = entry; created++; }
            else updated++;
            if (s.EventType is null) unmapped.Add($"{s.Year}-{s.Month:00}/{s.OrderNum}");
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[BattlePassMonthlyMissionImporter] +{created}/~{updated}");
        if (unmapped.Count > 0)
        {
            Console.WriteLine($"[BattlePassMonthlyMissionImporter] WARN: {unmapped.Count} rows " +
                              $"with no event_type: [{string.Join(", ", unmapped)}] — add name to " +
                              "BP_MONTHLY_EVENT_MAP in data_dumps/scripts/extract-bp-monthly-missions.py");
        }
        return created + updated;
    }
}
