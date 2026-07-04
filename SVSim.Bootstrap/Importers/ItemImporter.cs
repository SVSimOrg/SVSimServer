using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the item catalog from <c>seeds/items.json</c>. Source is the client's
/// <c>item_master.csv</c> + <c>itemtext.json</c> (extracted via
/// <c>data_dumps/scripts/extract-items.py</c>). Rows missing from the seed are LEFT INTACT.
/// </summary>
public class ItemImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "items.json");
        var seed = SeedLoader.LoadList<ItemSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[ItemImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.Items.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.ItemId == 0) continue;

            var entry = existing.TryGetValue(s.ItemId, out var ex)
                ? ex : new ItemEntry { Id = s.ItemId };

            entry.Name = s.Name;
            entry.Type = s.Type;
            entry.ThumbnailPath = s.ThumbnailPath;

            if (ex is null)
            {
                context.Items.Add(entry);
                existing[s.ItemId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[ItemImporter] +{created}/~{updated}");
        return created + updated;
    }
}
