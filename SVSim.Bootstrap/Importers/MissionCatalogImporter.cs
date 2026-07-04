using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of mission catalog rows from <c>seeds/mission-catalog.json</c>.
/// Rows missing from the seed are LEFT INTACT (so hand-added catalog rows survive).
/// </summary>
public class MissionCatalogImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<MissionCatalogSeed>(Path.Combine(seedDir, "mission-catalog.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[MissionCatalogImporter] No seed rows; skipping.");
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
                "[MissionCatalogImporter] seed rows reference unregistered event_type(s): "
                + string.Join(", ", unregistered)
                + ". Add the top-level prefix to MissionEventKeys.RegisteredPrefixes, or fix the seed.");
        }

        var existing = await context.MissionCatalog.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;
        var unmapped = new List<int>();
        foreach (var s in seed)
        {
            if (s.Id == 0) continue;
            var entry = existing.TryGetValue(s.Id, out var ex) ? ex : new MissionCatalogEntry { Id = s.Id };
            entry.Name = s.Name;
            entry.LotType = s.LotType;
            entry.RequireNumber = s.RequireNumber;
            entry.RewardType = (UserGoodsType)s.RewardType;
            entry.RewardDetailId = s.RewardDetailId;
            entry.RewardNumber = s.RewardNumber;
            entry.BattlePassPoint = s.BattlePassPoint;
            entry.DefaultFlag = s.DefaultFlag;
            entry.EventType = s.EventType;
            entry.EventArg = s.EventArg;
            entry.StartTime = s.StartTime;
            entry.EndTime = s.EndTime;
            if (ex is null) { context.MissionCatalog.Add(entry); existing[s.Id] = entry; created++; }
            else updated++;
            if (s.EventType is null) unmapped.Add(s.Id);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[MissionCatalogImporter] +{created}/~{updated}");
        if (unmapped.Count > 0)
        {
            Console.WriteLine($"[MissionCatalogImporter] WARN: {unmapped.Count} mission_ids with " +
                              $"no event_type: [{string.Join(", ", unmapped)}] — add to MISSION_EVENT_MAP " +
                              "in data_dumps/scripts/extract-missions.py and re-run the extractor");
        }
        return created + updated;
    }
}
