using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the basic-puzzle catalog from <c>seeds/puzzle-groups.json</c>,
/// <c>seeds/puzzles.json</c>, and <c>seeds/puzzle-missions.json</c>. Groups must be imported
/// before puzzles (FK on <see cref="PuzzleEntry.GroupId"/> -> <see cref="PuzzleGroupEntry.Id"/>).
/// Rows missing from the seed are LEFT INTACT (consistent with other per-importer seeds).
/// </summary>
public class PuzzleImporter
{
    public async Task<int> ImportGroupsAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "puzzle-groups.json");
        var seed = SeedLoader.LoadList<PuzzleGroupSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[PuzzleImporter] No group seed rows; skipping.");
            return 0;
        }

        var existing = await context.PuzzleGroups.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.Id == 0) continue;

            var entry = existing.TryGetValue(s.Id, out var ex)
                ? ex : new PuzzleGroupEntry { Id = s.Id };

            entry.BasicTitleTextId = s.BasicTitleTextId;
            entry.PuzzleCharaId = s.PuzzleCharaId;
            entry.CharaId = s.CharaId;
            entry.SortType = s.SortType;
            entry.DifficultyNameListJson = s.DifficultyNameList.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : JsonSerializer.Serialize(s.DifficultyNameList);

            if (ex is null)
            {
                context.PuzzleGroups.Add(entry);
                existing[s.Id] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PuzzleImporter] Groups +{created}/~{updated}");
        return created + updated;
    }

    public async Task<int> ImportPuzzlesAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "puzzles.json");
        var seed = SeedLoader.LoadList<PuzzleSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[PuzzleImporter] No puzzle seed rows; skipping.");
            return 0;
        }

        var existing = await context.Puzzles.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.Id == 0) continue;

            var entry = existing.TryGetValue(s.Id, out var ex)
                ? ex : new PuzzleEntry { Id = s.Id };

            entry.GroupId = s.GroupId;
            entry.PuzzleDifficulty = s.PuzzleDifficulty;
            entry.IsAdditional = s.IsAdditional;
            entry.IsPlayable = s.IsPlayable;
            entry.ReleaseConditionTextId = s.ReleaseConditionTextId;

            if (ex is null)
            {
                context.Puzzles.Add(entry);
                existing[s.Id] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PuzzleImporter] Puzzles +{created}/~{updated}");
        return created + updated;
    }

    public async Task<int> ImportMissionsAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "puzzle-missions.json");
        var seed = SeedLoader.LoadList<PuzzleMissionSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[PuzzleImporter] No mission seed rows; skipping.");
            return 0;
        }

        var existing = await context.PuzzleMissions.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.Id == 0) continue;

            var entry = existing.TryGetValue(s.Id, out var ex)
                ? ex : new PuzzleMissionEntry { Id = s.Id };

            entry.MissionName = s.MissionName;
            entry.AchievedMessage = s.AchievedMessage;
            entry.RequireNumber = s.RequireNumber;
            entry.CampaignCommenceTime = s.CampaignCommenceTime;
            entry.OrderId = s.OrderId;
            entry.RewardType = (UserGoodsType)s.RewardType;
            entry.RewardDetailId = s.RewardDetailId;
            entry.RewardNumber = s.RewardNumber;
            entry.TargetPuzzleGroupId = s.TargetPuzzleGroupId;

            if (ex is null)
            {
                context.PuzzleMissions.Add(entry);
                existing[s.Id] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PuzzleImporter] Missions +{created}/~{updated}");
        return created + updated;
    }
}
