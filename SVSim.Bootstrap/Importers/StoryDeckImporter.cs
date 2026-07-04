using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of story-deck presentation rows from <c>seeds/story-decks.json</c>.
/// Card lists are NOT imported here — they belong to BuildDeckProductEntry (deck_no == product_id),
/// so this importer should run AFTER BuildDeckImporter.ImportPackageAsync. Rows missing from the
/// seed are left intact.
/// </summary>
public class StoryDeckImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<StoryDeckSeed>(Path.Combine(seedDir, "story-decks.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[StoryDeckImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.StoryDecks.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.DeckNo == 0) continue;
            var entry = existing.TryGetValue(s.DeckNo, out var ex) ? ex : new StoryDeckEntry { DeckNo = s.DeckNo };
            entry.Kind = string.Equals(s.Kind, "trial", StringComparison.OrdinalIgnoreCase)
                ? StoryDeckKind.Trial : StoryDeckKind.Build;
            entry.ClassId = s.ClassId;
            entry.DeckName = s.DeckName;
            entry.SleeveId = s.SleeveId;
            entry.LeaderSkinId = s.LeaderSkinId;
            entry.IsRecommend = s.IsRecommend;
            entry.OrderNum = s.OrderNum;
            entry.EntryNo = s.EntryNo;
            entry.DeckFormat = s.DeckFormat;

            if (ex is null) { context.StoryDecks.Add(entry); existing[s.DeckNo] = entry; created++; }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[StoryDeckImporter] +{created}/~{updated}");
        return created + updated;
    }
}
