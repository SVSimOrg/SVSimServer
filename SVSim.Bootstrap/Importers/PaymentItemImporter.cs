using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of Steam payment items from <c>seeds/payment-items.json</c>.
/// Rows missing from the seed are LEFT INTACT.
/// </summary>
public class PaymentItemImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "payment-items.json");
        var seed = SeedLoader.LoadList<PaymentItemSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[PaymentItemImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.PaymentItems.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.RecordId == 0) continue;

            var entry = existing.TryGetValue(s.RecordId, out var ex)
                ? ex : new PaymentItemEntry { Id = s.RecordId };

            entry.ProductId = s.ProductId;
            entry.StoreProductId = s.StoreProductId;
            entry.Name = s.Name;
            entry.Text = s.Text;
            entry.Price = decimal.TryParse(s.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;
            entry.ChargeCrystalNum = s.ChargeCrystalNum;
            entry.FreeCrystalNum = s.FreeCrystalNum;
            entry.PurchaseLimit = s.PurchaseLimit;
            entry.SpecialShopFlag = s.SpecialShopFlag;
            entry.ImageName = s.ImageName;
            entry.StartTime = ImporterBase.ParseWireDateTime(s.StartTime);
            entry.EndTime = ImporterBase.ParseWireDateTime(s.EndTime);
            entry.RemainingTime = s.RemainingTime;
            entry.IsResaleProduct = s.IsResaleProduct;
            entry.ResaleStartDate = string.IsNullOrWhiteSpace(s.ResaleStartDate) ? null : ImporterBase.ParseWireDateTime(s.ResaleStartDate);

            if (ex is null)
            {
                context.PaymentItems.Add(entry);
                existing[s.RecordId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PaymentItemImporter] +{created}/~{updated}");
        return created + updated;
    }
}
