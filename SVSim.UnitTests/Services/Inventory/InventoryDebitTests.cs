using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryDebitTests
{
    [Test]
    public async Task Debit_Crystal_delegates_to_TrySpend()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 500;
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var r = await tx.TryDebitAsync(UserGoodsType.Crystal, 0, 200);

        Assert.That(r.Success, Is.True);
        Assert.That(r.PostStateTotal, Is.EqualTo(300));
    }

    [Test]
    public async Task Debit_Item_decrements_count_and_returns_post_state()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int itemId = 32000;
        var item = new ItemEntry { Id = itemId };
        ctx.Items.Add(item);
        var v = await ctx.Viewers.Include(x => x.Items).ThenInclude(i => i.Item).FirstAsync(x => x.Id == viewerId);
        v.Items.Add(new OwnedItemEntry { Item = item, Count = 10, Viewer = v });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var r = await tx.TryDebitAsync(UserGoodsType.Item, itemId, 3);

        Assert.That(r.Success, Is.True);
        Assert.That(r.PostStateTotal, Is.EqualTo(7));
    }

    [Test]
    public async Task Debit_Item_insufficient_returns_current_count_and_does_not_decrement()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int itemId = 32001;
        var item = new ItemEntry { Id = itemId };
        ctx.Items.Add(item);
        var v = await ctx.Viewers.Include(x => x.Items).ThenInclude(i => i.Item).FirstAsync(x => x.Id == viewerId);
        v.Items.Add(new OwnedItemEntry { Item = item, Count = 2, Viewer = v });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var r = await tx.TryDebitAsync(UserGoodsType.Item, itemId, 5);

        Assert.That(r.Outcome, Is.EqualTo(SpendOutcome.Insufficient));
        Assert.That(r.PostStateTotal, Is.EqualTo(2));
    }
}
