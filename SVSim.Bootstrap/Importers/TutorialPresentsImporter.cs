using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Loads the tutorial-gift catalogue (<c>tutorial-presents.json</c>) into the
/// <c>TutorialPresentEntries</c> table. Clear-and-rewrite — the seed file is authoritative;
/// hand-edits to the table are not preserved.
///
/// Read side: <c>ViewerRepository.RegisterAnonymousViewer</c> reads this table and projects
/// each row into a <c>ViewerPresent</c> with Source="tutorial" at signup time.
/// </summary>
public class TutorialPresentsImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<TutorialPresentSeed>(
            Path.Combine(seedDir, "tutorial-presents.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[TutorialPresentsImporter] No tutorial-present seed rows; skipping.");
            return 0;
        }

        var existing = await context.TutorialPresentEntries.ToListAsync();
        context.TutorialPresentEntries.RemoveRange(existing);

        foreach (var s in seed)
        {
            context.TutorialPresentEntries.Add(new TutorialPresentEntry
            {
                PresentId      = s.PresentId,
                RewardType     = s.RewardType,
                RewardDetailId = s.RewardDetailId,
                RewardCount    = s.RewardCount,
                ItemType       = s.ItemType,
                Message        = s.Message,
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[TutorialPresentsImporter] TutorialPresentEntries: -{existing.Count}/+{seed.Count}");
        return seed.Count;
    }
}
