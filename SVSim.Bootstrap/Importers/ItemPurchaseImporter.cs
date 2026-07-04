using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the item-purchase catalog from <c>seeds/item-purchase.json</c>.
/// Source is the wire <c>/item_purchase/info</c> response, extracted via
/// <c>data_dumps/scripts/extract-item-purchase.py</c>. Rows missing from the seed are LEFT INTACT.
/// </summary>
public class ItemPurchaseImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "item-purchase.json");
        var seed = SeedLoader.LoadList<ItemPurchaseSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[ItemPurchaseImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.ItemPurchaseCatalog.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.PurchaseId == 0) continue;

            var entry = existing.TryGetValue(s.PurchaseId, out var ex)
                ? ex : new ItemPurchaseCatalogEntry { Id = s.PurchaseId };

            entry.RequireItemType = s.RequireItemType;
            entry.RequireItemId = s.RequireItemId;
            entry.RequireItemNum = s.RequireItemNum;
            entry.PurchaseItemType = s.PurchaseItemType;
            entry.PurchaseItemId = s.PurchaseItemId;
            entry.PurchaseItemNum = s.PurchaseItemNum;
            entry.PurchaseName = s.PurchaseName;
            entry.IsMonthlyReset = s.IsMonthlyReset;
            entry.PurchaseLimit = s.PurchaseLimit;
            entry.IsEnabled = true;

            if (ex is null)
            {
                context.ItemPurchaseCatalog.Add(entry);
                existing[s.PurchaseId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[ItemPurchaseImporter] +{created}/~{updated}");
        return created + updated;
    }
}
