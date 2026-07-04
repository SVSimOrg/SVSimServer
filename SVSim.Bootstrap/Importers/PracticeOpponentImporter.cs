using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of practice opponents from <c>seeds/practice-opponents.json</c>.
/// Rows missing from the seed are LEFT INTACT (consistent with the previous import behavior;
/// a partial seed shouldn't silently delete entries).
/// </summary>
public class PracticeOpponentImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "practice-opponents.json");
        var seed = SeedLoader.LoadList<PracticeOpponentSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[PracticeOpponentImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.PracticeOpponents.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.PracticeId == 0) continue;

            var entry = existing.TryGetValue(s.PracticeId, out var ex)
                ? ex : new PracticeOpponentEntry { Id = s.PracticeId };

            entry.TextId = s.TextId;
            entry.ClassId = s.ClassId;
            entry.CharaId = s.CharaId;
            entry.DegreeId = s.DegreeId;
            entry.AiDeckLevel = s.AiDeckLevel;
            entry.AiLogicLevel = s.AiLogicLevel;
            entry.AiMaxLife = s.AiMaxLife;
            entry.Battle3dFieldId = s.Battle3dFieldId;
            entry.IsMaintenance = s.IsMaintenance;
            entry.IsCampaignPractice = s.IsCampaignPractice;

            if (ex is null)
            {
                context.PracticeOpponents.Add(entry);
                existing[s.PracticeId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PracticeOpponentImporter] +{created}/~{updated}");
        return created + updated;
    }
}
