using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Loads the prebuilt-deck catalog from a mix of client-master CSVs and one seed JSON.
/// Three methods run in dependency order (see Bootstrap/Program.cs):
///   1. ImportSeriesAsync  — build_deck_series_master.csv  → 22 series rows (all IsEnabled=false initially)
///   2. ImportCatalogAsync — seeds/build-deck-catalog.json  → enriches 7 series + 53 products
///       (tier backfill for missing intro/regular prices is performed by the extractor)
///   3. ImportPackageAsync — build_deck_package_master.csv → card lists for all 112 products,
///       creates disabled stubs for products not seeded by the catalog importer
/// Idempotent — re-runnable on the same files.
/// </summary>
public class BuildDeckImporter
{
    private const string BuildDeckSubdir = "build-deck";

    public async Task<int> ImportSeriesAsync(SVSimDbContext db, string dataDir)
    {
        string csvPath = Path.Combine(dataDir, BuildDeckSubdir, "build_deck_series_master.csv");
        if (!File.Exists(csvPath))
        {
            Console.Error.WriteLine($"[BuildDeckImporter] series CSV missing: {csvPath}");
            return 0;
        }

        var rows = ReadCsv(csvPath).Skip(1).ToList();   // skip header
        int created = 0, updated = 0;

        var existing = await db.BuildDeckSeries.ToDictionaryAsync(s => s.Id);
        foreach (var cols in rows)
        {
            if (cols.Length < 5) continue;
            if (!int.TryParse(cols[0], out int id)) continue;

            if (existing.TryGetValue(id, out var row))
            {
                // Update CSV-derived fields; do not flip IsEnabled or OrderIndex (catalog importer owns those)
                bool changed = false;
                if (row.NameKey != cols[1])      { row.NameKey = cols[1]; changed = true; }
                if (row.IntroKey != cols[2])     { row.IntroKey = cols[2]; changed = true; }
                if (row.TitlePath != cols[3])    { row.TitlePath = cols[3]; changed = true; }
                if (row.DrumrollPath != cols[4]) { row.DrumrollPath = cols[4]; changed = true; }
                if (changed) updated++;
            }
            else
            {
                db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
                {
                    Id           = id,
                    NameKey      = cols[1],
                    IntroKey     = cols[2],
                    TitlePath    = cols[3],
                    DrumrollPath = cols[4],
                    OrderIndex   = 0,
                    IsNew        = false,
                    IsEnabled    = false,
                });
                created++;
            }
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"[BuildDeckImporter] Series: created={created}, updated={updated}");
        return created + updated;
    }

    public async Task<int> ImportPackageAsync(SVSimDbContext db, string dataDir)
    {
        string csvPath = Path.Combine(dataDir, BuildDeckSubdir, "build_deck_package_master.csv");
        if (!File.Exists(csvPath))
        {
            Console.Error.WriteLine($"[BuildDeckImporter] package CSV missing: {csvPath}");
            return 0;
        }

        var rows = ReadCsv(csvPath).Skip(1).ToList();   // header: product_id,card_id,number,is_spot
        var byProduct = rows
            .Where(c => c.Length >= 4)
            .GroupBy(c => int.Parse(c[0]))
            .ToDictionary(g => g.Key, g => g.Select(c => new BuildDeckProductCardEntry
            {
                CardId = long.Parse(c[1]),
                Number = int.Parse(c[2]),
                IsSpot = int.Parse(c[3]) != 0,
            }).ToList());

        // Load existing products (we may have stubs from a prior run or rows created by catalog importer)
        var existing = await db.BuildDeckProducts.Include(p => p.Cards).ToDictionaryAsync(p => p.Id);
        int created = 0, updated = 0;

        foreach (var (productId, cardEntries) in byProduct)
        {
            if (existing.TryGetValue(productId, out var product))
            {
                // Replace card list wholesale — CSV is authoritative.
                product.Cards.Clear();
                foreach (var c in cardEntries) product.Cards.Add(c);
                updated++;
            }
            else
            {
                int? seriesId = InferSeriesId(productId);
                if (seriesId is null)
                {
                    Console.Error.WriteLine($"[BuildDeckImporter] product {productId} has no inferable series; skipping");
                    continue;
                }
                db.BuildDeckProducts.Add(new BuildDeckProductEntry
                {
                    Id                  = productId,
                    SeriesId            = seriesId.Value,
                    LeaderId            = 0,
                    DeckCode            = string.Empty,
                    ProductNameKey      = string.Empty,
                    FeaturedCardId      = 0,
                    PurchaseNumMax      = 1,
                    IntroPriceCrystal   = null,
                    RegularPriceCrystal = null,
                    IntroPriceRupy      = null,
                    RegularPriceRupy    = null,
                    IsEnabled           = false,
                    Cards               = cardEntries,
                });
                created++;
            }
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"[BuildDeckImporter] Package: created={created}, updated={updated}");
        return created + updated;
    }

    public async Task<int> ImportCatalogAsync(SVSimDbContext db, string seedDir)
    {
        var seed = SeedLoader.LoadList<BuildDeckCatalogSeed>(Path.Combine(seedDir, "build-deck-catalog.json"));
        if (seed.Count == 0) return 0;

        int touchedSeries = 0, touchedProducts = 0;

        var existingSeries = await db.BuildDeckSeries
            .Include(s => s.SeriesRewards)
            .ToDictionaryAsync(s => s.Id);
        var existingProducts = await db.BuildDeckProducts
            .Include(p => p.Rewards)
            .ToDictionaryAsync(p => p.Id);

        foreach (var s in seed)
        {
            if (s.SeriesId == 0) continue;

            if (!existingSeries.TryGetValue(s.SeriesId, out var seriesRow))
            {
                // Catalog typically runs after the series CSV; if a seed series isn't in the
                // CSV we create a bare stub so the FK from products holds.
                seriesRow = new BuildDeckSeriesEntry
                {
                    Id = s.SeriesId, NameKey = string.Empty, IntroKey = string.Empty,
                    TitlePath = string.Empty, DrumrollPath = string.Empty,
                };
                db.BuildDeckSeries.Add(seriesRow);
                existingSeries[s.SeriesId] = seriesRow;
            }
            seriesRow.OrderIndex = s.OrderId;
            seriesRow.IsNew = s.IsNew;
            seriesRow.IsEnabled = true;

            seriesRow.SeriesRewards.Clear();
            foreach (var r in s.SeriesRewards)
            {
                seriesRow.SeriesRewards.Add(new BuildDeckSeriesRewardEntry
                {
                    TierIndex = r.TierIndex,
                    ItemIndex = r.ItemIndex,
                    RewardType = (UserGoodsType)r.RewardType,
                    RewardDetailId = r.RewardDetailId,
                    RewardNumber = r.RewardNumber,
                    MessageId = r.MessageId,
                });
            }
            touchedSeries++;

            foreach (var p in s.Products)
            {
                if (!existingProducts.TryGetValue(p.ProductId, out var productRow))
                {
                    productRow = new BuildDeckProductEntry { Id = p.ProductId, SeriesId = s.SeriesId };
                    db.BuildDeckProducts.Add(productRow);
                    existingProducts[p.ProductId] = productRow;
                }
                productRow.SeriesId = s.SeriesId;
                productRow.LeaderId = p.LeaderId;
                productRow.DeckCode = p.DeckCode;
                productRow.ProductNameKey = p.ProductName;
                productRow.FeaturedCardId = p.FeaturedCardId;
                productRow.PurchaseNumMax = p.PurchaseNumMax;
                productRow.IsEnabled = true;
                productRow.IntroPriceCrystal = p.IntroPriceCrystal;
                productRow.RegularPriceCrystal = p.RegularPriceCrystal;
                productRow.IntroPriceRupy = p.IntroPriceRupy;
                productRow.RegularPriceRupy = p.RegularPriceRupy;

                productRow.Rewards.Clear();
                foreach (var r in p.Rewards)
                {
                    productRow.Rewards.Add(new BuildDeckProductRewardEntry
                    {
                        RewardIndex = r.RewardIndex,
                        RewardType = (UserGoodsType)r.RewardType,
                        RewardDetailId = r.RewardDetailId,
                        RewardNumber = r.RewardNumber,
                        MessageId = r.MessageId,
                    });
                }
                touchedProducts++;
            }
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"[BuildDeckImporter] Catalog: series={touchedSeries}, products={touchedProducts}");
        return touchedSeries + touchedProducts;
    }

    /// <summary>
    /// Maps a product_id to its series_id using the numeric pattern derived from the /info capture
    /// and CSV inspection.
    ///   Sets 1–7: products 1–7, 201–299, 301–399, 401–499, 501–599, 601–699, 701–799 → series 101–107
    ///   Temporary Deck: products 10001–10099 → series 10100
    ///   Trial series: products NNxx where NN in [119,…,132] → series NN00 (divide-by-100 * 100)
    /// </summary>
    internal static int? InferSeriesId(int productId) => productId switch
    {
        >= 1     and <= 7     => 101,
        >= 201   and <= 299   => 102,
        >= 301   and <= 399   => 103,
        >= 401   and <= 499   => 104,
        >= 501   and <= 599   => 105,
        >= 601   and <= 699   => 106,
        >= 701   and <= 799   => 107,
        >= 10001 and <= 10099 => 10100,
        >= 11901 and <= 13299 => (productId / 100) * 100,
        _ => null,
    };

    private static IEnumerable<string[]> ReadCsv(string path)
    {
        foreach (var raw in File.ReadAllLines(path, System.Text.Encoding.UTF8))
        {
            // Strip UTF-8 BOM on the first line if present
            var line = raw.TrimStart('﻿');
            if (string.IsNullOrWhiteSpace(line)) continue;
            yield return line.Split(',');
        }
    }
}
