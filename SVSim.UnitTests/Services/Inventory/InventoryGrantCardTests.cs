using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryGrantCardTests
{
    [Test]
    public async Task Card_first_grant_creates_owned_with_post_state_count()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();   // helper added below if missing
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        var granted = await tx.GrantAsync(UserGoodsType.Card, cardId, 2);

        Assert.That(granted, Has.Count.EqualTo(1));
        Assert.That(granted[0].RewardType, Is.EqualTo(UserGoodsType.Card));
        Assert.That(granted[0].RewardId, Is.EqualTo(cardId));
        Assert.That(granted[0].RewardNum, Is.EqualTo(2));
    }

    [Test]
    public async Task Card_cascade_grants_associated_cosmetic_and_appends_entry()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_010_000;
        ctx.Sleeves.Add(new SleeveEntry { Id = sleeveId });
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Card, cardId, 1);

        Assert.That(granted, Has.Count.EqualTo(2));
        Assert.That(granted[1].RewardType, Is.EqualTo(UserGoodsType.Sleeve));
        Assert.That(granted[1].RewardId, Is.EqualTo(sleeveId));
    }

    [Test]
    public async Task Card_cascade_skips_already_owned_cosmetic()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_010_001;
        var sleeve = new SleeveEntry { Id = sleeveId };
        ctx.Sleeves.Add(sleeve);
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        var v = await ctx.Viewers.Include(x => x.Sleeves).FirstAsync(x => x.Id == viewerId);
        v.Sleeves.Add(sleeve);
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var granted = await tx.GrantAsync(UserGoodsType.Card, cardId, 1);

        Assert.That(granted, Has.Count.EqualTo(1), "owned cosmetic skipped from cascade");
    }
}
