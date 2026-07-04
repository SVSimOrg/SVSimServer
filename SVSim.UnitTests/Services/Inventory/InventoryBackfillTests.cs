using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryBackfillTests
{
    [Test]
    public async Task Backfill_grants_missing_cosmetic_for_already_owned_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int sleeveId = 2_000_020_000;
        ctx.Sleeves.Add(new SleeveEntry { Id = sleeveId });
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        var card = await ctx.Cards.FirstAsync(c => c.Id == cardId);
        var v = await ctx.Viewers.Include(x => x.Cards).ThenInclude(c => c.Card).FirstAsync(x => x.Id == viewerId);
        v.Cards.Add(new OwnedCardEntry { Card = card, Count = 3, IsProtected = false });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        int granted = await tx.BackfillCardCosmeticsAsync();

        Assert.That(granted, Is.EqualTo(1));
        Assert.That(tx.Viewer.Sleeves.Any(s => s.Id == sleeveId), Is.True);
    }

    [Test]
    public async Task Backfill_idempotent_on_already_owned_cosmetic()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int sleeveId = 2_000_020_001;
        var sleeve = new SleeveEntry { Id = sleeveId };
        ctx.Sleeves.Add(sleeve);
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        var card = await ctx.Cards.FirstAsync(c => c.Id == cardId);
        var v = await ctx.Viewers
            .Include(x => x.Cards).ThenInclude(c => c.Card)
            .Include(x => x.Sleeves)
            .FirstAsync(x => x.Id == viewerId);
        v.Cards.Add(new OwnedCardEntry { Card = card, Count = 3, IsProtected = false });
        v.Sleeves.Add(sleeve);
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        int granted = await tx.BackfillCardCosmeticsAsync();

        Assert.That(granted, Is.EqualTo(0));
    }
}
