using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class LeaderSkinShopImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_series_products_and_set_rewards_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new LeaderSkinShopImporter().ImportAsync(db, SeedDir);

        var series = await db.LeaderSkinShopSeries
            .Include(s => s.SetCompletionRewards)
            .Include(s => s.Products).ThenInclude(p => p.Rewards)
            .OrderBy(s => s.Id)
            .ToListAsync();

        Assert.That(series.Count, Is.GreaterThan(0));

        // Spot-check series 100 (Shingeki no Bahamut) — set sale active with 2000c/2000r set price
        var s100 = series.First(s => s.Id == 100);
        Assert.That(s100.SetSalesStatus, Is.EqualTo(1));
        Assert.That(s100.SetPriceCrystal, Is.EqualTo(2000));
        Assert.That(s100.SetPriceRupy, Is.EqualTo(2000));
        Assert.That(s100.Products.Count, Is.GreaterThan(0));

        // Spot-check a series with set-completion rewards (series 103 in capture has 2)
        var withRewards = series.FirstOrDefault(s => s.SetCompletionRewards.Count > 0);
        Assert.That(withRewards, Is.Not.Null, "at least one series should have set-completion rewards");
        Assert.That(withRewards!.SetCompletionRewards.All(r => r.RewardDetailId > 0), Is.True);

        // Per-product rewards (the captured shape — skin + emblem + sleeve triplet)
        var firstProduct = s100.Products.OrderBy(p => p.Id).First();
        Assert.That(firstProduct.LeaderSkinId, Is.GreaterThan(0));
        Assert.That(firstProduct.Rewards.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new LeaderSkinShopImporter().ImportAsync(db, SeedDir);
        int seriesBefore = await db.LeaderSkinShopSeries.CountAsync();
        int productsBefore = await db.LeaderSkinShopProducts.CountAsync();

        await new LeaderSkinShopImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.LeaderSkinShopSeries.CountAsync(), Is.EqualTo(seriesBefore));
        Assert.That(await db.LeaderSkinShopProducts.CountAsync(), Is.EqualTo(productsBefore));
    }
}
