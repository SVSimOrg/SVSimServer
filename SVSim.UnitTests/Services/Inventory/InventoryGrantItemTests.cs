using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryGrantItemTests
{
    [Test]
    public async Task Item_first_grant_creates_owned_entry()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int itemId = 31000;
        ctx.Items.Add(new ItemEntry { Id = itemId });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Item, itemId, 3);

        Assert.That(granted[0].RewardNum, Is.EqualTo(3));
        Assert.That(tx.Viewer.Items.Single(i => i.Item.Id == itemId).Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Item_second_grant_accumulates_post_state()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int itemId = 31001;
        var item = new ItemEntry { Id = itemId };
        ctx.Items.Add(item);
        var v = await ctx.Viewers.Include(x => x.Items).ThenInclude(i => i.Item).FirstAsync(x => x.Id == viewerId);
        v.Items.Add(new OwnedItemEntry { Item = item, Count = 5, Viewer = v });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Item, itemId, 4);

        Assert.That(granted[0].RewardNum, Is.EqualTo(9));
        Assert.That(tx.Viewer.Items.Single(i => i.Item.Id == itemId).Count, Is.EqualTo(9));
    }
}
