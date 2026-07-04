using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of AI bot opponents from <c>seeds/bot-roster.json</c>.
/// Rows missing from the seed are LEFT INTACT (consistent with PracticeOpponentImporter;
/// a partial seed shouldn't silently delete entries).
/// </summary>
public class BotRosterImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "bot-roster.json");
        var seed = SeedLoader.LoadList<BotRosterSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[BotRosterImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.BotRoster.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.AiId == 0) continue;

            var entry = existing.TryGetValue(s.AiId, out var ex)
                ? ex : new BotRosterEntry { Id = s.AiId };

            entry.CountryCode = s.CountryCode;
            entry.UserName = s.UserName;
            entry.SleeveId = s.SleeveId;
            entry.EmblemId = s.EmblemId;
            entry.DegreeId = s.DegreeId;
            entry.FieldId = s.FieldId;
            entry.IsOfficial = s.IsOfficial;
            entry.ClassId = s.ClassId;
            entry.CharaId = s.CharaId;
            entry.Rank = s.Rank;
            entry.BattlePoint = s.BattlePoint;
            entry.IsMasterRank = s.IsMasterRank;
            entry.MasterPoint = s.MasterPoint;

            if (ex is null)
            {
                context.BotRoster.Add(entry);
                existing[s.AiId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[BotRosterImporter] +{created}/~{updated}");
        return created + updated;
    }
}
