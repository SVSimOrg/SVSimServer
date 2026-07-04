using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryGrantCurrencyTests
{
    [TestCase(UserGoodsType.Rupy)]
    [TestCase(UserGoodsType.Crystal)]
    [TestCase(UserGoodsType.RedEther)]
    [TestCase(UserGoodsType.SpotCardPoint)]
    public async Task Grant_currency_adds_and_emits_post_state(UserGoodsType type)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        switch (type)
        {
            case UserGoodsType.Rupy: v.Currency.Rupees = 100; break;
            case UserGoodsType.Crystal: v.Currency.Crystals = 100; break;
            case UserGoodsType.RedEther: v.Currency.RedEther = 100; break;
            case UserGoodsType.SpotCardPoint: v.Currency.SpotPoints = 100; break;
        }
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        var granted = await tx.GrantAsync(type, detailId: 0, num: 50);

        Assert.That(granted, Has.Count.EqualTo(1));
        Assert.That(granted[0].RewardType, Is.EqualTo(type));
        Assert.That(granted[0].RewardNum, Is.EqualTo(150));
    }
}
