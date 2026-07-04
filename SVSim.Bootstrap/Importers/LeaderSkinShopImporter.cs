using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the leader-skin-shop catalog from <c>seeds/leader-skin-shop.json</c>.
/// Mirror of <see cref="SleeveShopImporter"/>. Source is the wire
/// <c>/leader_skin/products</c> response, extracted via
/// <c>data_dumps/scripts/extract-leader-skin-shop.py</c>. Rows missing from the seed are LEFT INTACT.
/// </summary>
public class LeaderSkinShopImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "leader-skin-shop.json");
        var seed = SeedLoader.LoadList<LeaderSkinShopSeriesSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[LeaderSkinShopImporter] No seed rows; skipping.");
            return 0;
        }

        var existingSeries = await context.LeaderSkinShopSeries
            .Include(s => s.SetCompletionRewards)
            .Include(s => s.Products).ThenInclude(p => p.Rewards)
            .ToDictionaryAsync(s => s.Id);

        int createdSeries = 0, updatedSeries = 0, createdProducts = 0, updatedProducts = 0;

        foreach (var s in seed)
        {
            if (s.SeriesId == 0) continue;

            if (!existingSeries.TryGetValue(s.SeriesId, out var series))
            {
                series = new LeaderSkinShopSeriesEntry { Id = s.SeriesId };
                context.LeaderSkinShopSeries.Add(series);
                existingSeries[s.SeriesId] = series;
                createdSeries++;
            }
            else updatedSeries++;

            series.IsNew = s.IsNew;
            series.IsEnabled = true;
            series.SetSalesStatus = s.SetSalesStatus;
            series.SetPriceCrystal = s.SetPriceCrystal;
            series.SetPriceRupy = s.SetPriceRupy;
            series.SetPriceTicket = s.SetPriceTicket;
            series.SetPriceTicketId = s.SetPriceTicketId;
            // SetCompletionRewardStatus stays at the catalog default 0 — per-viewer claim state
            // is computed at request time from ViewerLeaderSkinSetClaim, not from this column.
            series.SetCompletionRewardStatus = 0;

            // Replace owned collections wholesale on rerun.
            series.SetCompletionRewards.Clear();
            foreach (var r in s.SetCompletionRewards.OrderBy(r => r.OrderIndex))
            {
                series.SetCompletionRewards.Add(new LeaderSkinShopSeriesRewardEntry
                {
                    OrderIndex = r.OrderIndex,
                    RewardType = (UserGoodsType)r.RewardType,
                    RewardDetailId = r.RewardDetailId,
                    RewardNumber = r.RewardNumber,
                });
            }

            var existingProducts = series.Products.ToDictionary(p => p.Id);
            foreach (var p in s.Products)
            {
                if (p.ProductId == 0) continue;

                if (!existingProducts.TryGetValue(p.ProductId, out var product))
                {
                    product = new LeaderSkinShopProductEntry { Id = p.ProductId };
                    series.Products.Add(product);
                    createdProducts++;
                }
                else updatedProducts++;

                product.SeriesId = s.SeriesId;
                product.LeaderSkinId = p.LeaderSkinId;
                product.ProductNameKey = p.ProductNameKey;
                product.IntroductionKey = p.IntroductionKey;
                product.CvNameKey = p.CvNameKey;
                product.SinglePriceCrystal = p.SinglePriceCrystal;
                product.SinglePriceRupy = p.SinglePriceRupy;
                product.SinglePriceTicket = p.SinglePriceTicket;
                product.TicketNumber = p.TicketNumber;
                product.TicketItemId = p.TicketItemId;
                product.IsEnabled = true;

                product.Rewards.Clear();
                foreach (var r in p.Rewards.OrderBy(r => r.OrderIndex))
                {
                    product.Rewards.Add(new LeaderSkinShopProductRewardEntry
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
            $"[LeaderSkinShopImporter] series +{createdSeries}/~{updatedSeries}, " +
            $"products +{createdProducts}/~{updatedProducts}");
        return createdSeries + updatedSeries;
    }
}
