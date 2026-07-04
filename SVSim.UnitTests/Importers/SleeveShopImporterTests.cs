using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class SleeveShopImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_series_and_products_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new SleeveShopImporter().ImportAsync(db, SeedDir);

        var series = await db.SleeveShopSeries
            .Include(s => s.Products).ThenInclude(p => p.Rewards)
            .OrderBy(s => s.Id)
            .ToListAsync();

        Assert.That(series.Count, Is.GreaterThan(0), "seed file should contain series");
        // Spot-check series 3019 (BattlePass sleeves) — captured at 6 products with crystal pricing.
        var bp = series.FirstOrDefault(s => s.Id == 3019);
        Assert.That(bp, Is.Not.Null, "series 3019 should be present");
        Assert.That(bp!.Products.Count, Is.GreaterThan(0));

        var firstProduct = bp.Products.OrderBy(p => p.Id).First();
        Assert.That(firstProduct.NameKey, Does.StartWith("sleeve_"), "name should be a SystemText key");
        Assert.That(firstProduct.Rewards, Is.Not.Empty, "products should have catalog rewards");
        Assert.That(firstProduct.IsEnabled, Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new SleeveShopImporter().ImportAsync(db, SeedDir);
        int seriesBefore = await db.SleeveShopSeries.CountAsync();
        int productsBefore = await db.SleeveShopProducts.CountAsync();

        await new SleeveShopImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.SleeveShopSeries.CountAsync(), Is.EqualTo(seriesBefore));
        Assert.That(await db.SleeveShopProducts.CountAsync(), Is.EqualTo(productsBefore));
    }

    [Test]
    public async Task Replaces_rewards_wholesale_on_rerun()
    {
        // Owned rewards collection: importer clears and re-adds. A stale catalog reward should
        // not survive a re-import. (Hand-tamper one row, re-import, check the tamper is gone.)
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new SleeveShopImporter().ImportAsync(db, SeedDir);
        var product = await db.SleeveShopProducts
            .Include(p => p.Rewards)
            .OrderBy(p => p.Id)
            .FirstAsync();

        int originalCount = product.Rewards.Count;
        product.Rewards.Add(new SleeveShopProductRewardEntry
        {
            OrderIndex = 99, RewardType = (UserGoodsType)99, RewardDetailId = 99, RewardNumber = 99,
        });
        await db.SaveChangesAsync();

        await new SleeveShopImporter().ImportAsync(db, SeedDir);

        var reloaded = await db.SleeveShopProducts
            .Include(p => p.Rewards)
            .FirstAsync(p => p.Id == product.Id);
        Assert.That(reloaded.Rewards.Count, Is.EqualTo(originalCount), "extra reward should be wiped on re-import");
        Assert.That(reloaded.Rewards.Any(r => r.RewardType == (UserGoodsType)99), Is.False);
    }
}
