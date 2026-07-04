using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventorySpendTests
{
    [Test]
    public async Task Spend_sufficient_returns_post_deduction_total()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 1000;
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var r = await tx.TrySpendAsync(SpendCurrency.Crystal, 300);

        Assert.That(r.Success, Is.True);
        Assert.That(r.PostStateTotal, Is.EqualTo(700));
        Assert.That(tx.Viewer.Currency.Crystals, Is.EqualTo(700UL));
    }

    [Test]
    public async Task Spend_insufficient_returns_insufficient_with_current_balance()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 100;
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var r = await tx.TrySpendAsync(SpendCurrency.Crystal, 300);

        Assert.That(r.Outcome, Is.EqualTo(SpendOutcome.Insufficient));
        Assert.That(r.PostStateTotal, Is.EqualTo(100));
        Assert.That(tx.Viewer.Currency.Crystals, Is.EqualTo(100UL), "balance unchanged");
    }

    [Test]
    public async Task Freeplay_returns_success_with_configured_amount_for_main_currencies()
    {
        using var factory = new SVSimTestFactory(freeplayEnabled: true);
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        ulong balanceBefore = tx.Viewer.Currency.Crystals;
        var r = await tx.TrySpendAsync(SpendCurrency.Crystal, 99999);

        Assert.That(r.Success, Is.True);
        var freeCfg = scope.ServiceProvider.GetRequiredService<IGameConfigService>().Get<FreeplayConfig>();
        Assert.That(r.PostStateTotal, Is.EqualTo(checked((long)freeCfg.CurrencyAmount)));
        Assert.That(tx.Viewer.Currency.Crystals, Is.EqualTo(balanceBefore), "freeplay never deducts");
    }
}
