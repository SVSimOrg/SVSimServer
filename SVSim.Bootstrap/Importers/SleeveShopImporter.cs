using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the sleeve-shop catalog from <c>seeds/sleeve-shop.json</c>.
/// Source is the wire <c>/sleeve/info</c> response, extracted via
/// <c>data_dumps/scripts/extract-sleeve-shop.py</c>. Mirror of the BuildDeck importer pattern.
/// Rows missing from the seed are LEFT INTACT (so manual test fixtures survive re-runs).
/// </summary>
public class SleeveShopImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "sleeve-shop.json");
        var seed = SeedLoader.LoadList<SleeveShopSeriesSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[SleeveShopImporter] No seed rows; skipping.");
            return 0;
        }

        var existingSeries = await context.SleeveShopSeries
            .Include(s => s.Products).ThenInclude(p => p.Rewards)
            .ToDictionaryAsync(s => s.Id);

        int createdSeries = 0, updatedSeries = 0, createdProducts = 0, updatedProducts = 0;

        foreach (var s in seed)
        {
            if (s.SeriesId == 0) continue;

            if (!existingSeries.TryGetValue(s.SeriesId, out var series))
            {
                series = new SleeveShopSeriesEntry { Id = s.SeriesId };
                context.SleeveShopSeries.Add(series);
                existingSeries[s.SeriesId] = series;
                createdSeries++;
            }
            else updatedSeries++;

            series.IsNew = s.IsNew;
            series.IsEnabled = true;

            var existingProducts = series.Products.ToDictionary(p => p.Id);
            foreach (var p in s.Products)
            {
                if (p.ProductId == 0) continue;

                if (!existingProducts.TryGetValue(p.ProductId, out var product))
                {
                    product = new SleeveShopProductEntry { Id = p.ProductId };
                    series.Products.Add(product);
                    createdProducts++;
                }
                else updatedProducts++;

                product.SeriesId = s.SeriesId;
                product.NameKey = p.NameKey;
                product.PriceCrystal = p.PriceCrystal;
                product.PriceRupy = p.PriceRupy;
                product.IsEnabled = true;

                // Rewards: replace wholesale (owned collection — EF will issue DELETE+INSERT
                // anyway, and the wire shape is canonical per re-extract).
                product.Rewards.Clear();
                foreach (var r in p.Rewards.OrderBy(r => r.OrderIndex))
                {
                    product.Rewards.Add(new SleeveShopProductRewardEntry
                    {
                        OrderIndex = r.OrderIndex,
                        RewardType = (UserGoodsType)r.RewardType,
                        RewardDetailId = r.RewardDetailId,
                        RewardNumber = r.RewardNumber,
                    });
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine(
            $"[SleeveShopImporter] series +{createdSeries}/~{updatedSeries}, " +
            $"products +{createdProducts}/~{updatedProducts}");
        return createdSeries + updatedSeries;
    }
}
