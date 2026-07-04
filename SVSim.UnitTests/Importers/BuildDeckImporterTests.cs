using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BuildDeckImporterTests
{
    private static string DataDir => Path.Combine(AppContext.BaseDirectory, "Data");
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task ImportsAll22Series_with_22_disabled_until_catalog_enables()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BuildDeckImporter().ImportSeriesAsync(db, DataDir);

        var series = await db.BuildDeckSeries.OrderBy(s => s.Id).ToListAsync();
        Assert.That(series.Count, Is.EqualTo(22));
        Assert.That(series.All(s => !s.IsEnabled), Is.True, "all series disabled until catalog importer runs");
        Assert.That(series.Any(s => s.NameKey.StartsWith("BDSSN_")), Is.True);
    }

    [Test]
    public async Task ImportPackage_creates_stub_products_with_inferred_series_and_full_card_lists()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BuildDeckImporter().ImportSeriesAsync(db, DataDir);
        await new BuildDeckImporter().ImportPackageAsync(db, DataDir);

        var products = await db.BuildDeckProducts.Include(p => p.Cards).ToListAsync();
        Assert.That(products.Count, Is.EqualTo(112), "stubs for all 112 products");
        Assert.That(products.All(p => !p.IsEnabled), Is.True, "stubs are disabled until catalog enables");
        Assert.That(products.All(p => p.Cards.Sum(c => c.Number) == 40), Is.True, "every product is a 40-card deck");

        // Spot-check a known mapping: product 1 -> series 101 via the InferSeriesId helper.
        var p1 = products.Single(p => p.Id == 1);
        Assert.That(p1.SeriesId, Is.EqualTo(101));
    }

    [Test]
    public async Task Importer_is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new BuildDeckImporter();
        await importer.ImportSeriesAsync(db, DataDir);
        await importer.ImportPackageAsync(db, DataDir);
        await importer.ImportSeriesAsync(db, DataDir);
        await importer.ImportPackageAsync(db, DataDir);

        Assert.That(await db.BuildDeckSeries.CountAsync(), Is.EqualTo(22));
        Assert.That(await db.BuildDeckProducts.CountAsync(), Is.EqualTo(112));
    }

    [Test]
    public async Task ImportCatalog_enriches_7_captured_series_with_prices_and_tiers()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new BuildDeckImporter();
        await importer.ImportSeriesAsync(db, DataDir);
        await importer.ImportCatalogAsync(db, SeedDir);
        await importer.ImportPackageAsync(db, DataDir);

        // Series 101 (Set 1) should be enabled and order_id=22 from capture
        var s101 = await db.BuildDeckSeries
            .Include(s => s.Products).ThenInclude(p => p.Cards)
            .Include(s => s.Products).ThenInclude(p => p.Rewards)
            .Include(s => s.SeriesRewards)
            .FirstAsync(s => s.Id == 101);
        Assert.That(s101.IsEnabled, Is.True);
        Assert.That(s101.OrderIndex, Is.EqualTo(22));
        Assert.That(s101.Products.Count, Is.EqualTo(7), "Set 1 has 7 products (no Nemesis)");

        // Set 1 products: max=3, intro=500 backfilled from siblings, regular=750 backfilled from siblings
        var product1 = s101.Products.Single(p => p.Id == 1);
        Assert.That(product1.IsEnabled, Is.True);
        Assert.That(product1.PurchaseNumMax, Is.EqualTo(3));
        Assert.That(product1.IntroPriceCrystal, Is.EqualTo(500));
        Assert.That(product1.RegularPriceCrystal, Is.EqualTo(750));

        // Series 107 (Set 7) products: max=1, intro=1200, regular=null
        var s107 = await db.BuildDeckSeries
            .Include(s => s.Products)
            .FirstAsync(s => s.Id == 107);
        Assert.That(s107.Products.All(p => p.PurchaseNumMax == 1), Is.True);
        Assert.That(s107.Products.All(p => p.IntroPriceCrystal == 1200), Is.True);
        Assert.That(s107.Products.All(p => p.RegularPriceCrystal == null), Is.True);

        // Series 105 should have populated series-reward tiers (from the capture)
        var s105 = await db.BuildDeckSeries.Include(s => s.SeriesRewards).FirstAsync(s => s.Id == 105);
        Assert.That(s105.SeriesRewards.Count, Is.GreaterThan(0), "Set 5 has series-reward tiers");

        // Series 10100 (Temporary Deck) should still be disabled — not in capture
        var sTemp = await db.BuildDeckSeries.FirstAsync(s => s.Id == 10100);
        Assert.That(sTemp.IsEnabled, Is.False);
    }
}
