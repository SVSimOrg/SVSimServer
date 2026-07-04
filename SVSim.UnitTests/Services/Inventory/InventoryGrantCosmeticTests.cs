using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryGrantCosmeticTests
{
    [Test]
    public async Task Sleeve_added_when_missing()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_000_001;
        ctx.Sleeves.Add(new SleeveEntry { Id = sleeveId });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Sleeve, sleeveId, 1);

        Assert.That(granted, Has.Count.EqualTo(1));
        Assert.That(granted[0].RewardType, Is.EqualTo(UserGoodsType.Sleeve));
        Assert.That(granted[0].RewardId, Is.EqualTo(sleeveId));
        Assert.That(granted[0].RewardNum, Is.EqualTo(1));
        Assert.That(tx.Viewer.Sleeves.Any(s => s.Id == sleeveId), Is.True);
    }

    [Test]
    public async Task Sleeve_idempotent_when_already_owned_but_still_emits_entry()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_000_002;
        var sleeve = new SleeveEntry { Id = sleeveId };
        ctx.Sleeves.Add(sleeve);
        var v = await ctx.Viewers.Include(x => x.Sleeves).FirstAsync(x => x.Id == viewerId);
        v.Sleeves.Add(sleeve);
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Sleeve, sleeveId, 1);

        Assert.That(granted, Has.Count.EqualTo(1), "top-level cosmetic grant emits even if owned");
        Assert.That(tx.Viewer.Sleeves.Count(s => s.Id == sleeveId), Is.EqualTo(1), "no duplicate row");
    }

    [Test]
    public async Task Unknown_cosmetic_id_throws_catalog_exception()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        Assert.ThrowsAsync<InventoryCatalogException>(
            async () => { await tx.GrantAsync(UserGoodsType.Sleeve, 999_999, 1); });
    }
}
