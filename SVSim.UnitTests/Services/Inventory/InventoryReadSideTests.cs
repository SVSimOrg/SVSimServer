using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryReadSideTests
{
    [Test]
    public async Task EffectiveBalance_returns_viewer_currency_when_not_freeplay()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 1234;
        await ctx.SaveChangesAsync();

        // Re-load viewer with inventory graph for the read-side call
        var v2 = await ctx.Viewers.AsNoTracking().FirstAsync(x => x.Id == viewerId);
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        Assert.That(inv.EffectiveBalance(v2, SpendCurrency.Crystal), Is.EqualTo(1234));
    }

    [Test]
    public async Task EffectiveOwnedCardsAsync_returns_non_null_collection()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers
            .Include(x => x.Cards).ThenInclude(c => c.Card)
            .FirstAsync(x => x.Id == viewerId);

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var owned = await inv.EffectiveOwnedCardsAsync(v);

        Assert.That(owned, Is.Not.Null);
        // If there are basic cards seeded (IsBasic=true) they should be protected;
        // if none are seeded the collection may be empty — just confirm it doesn't throw.
    }

    [Test]
    public async Task EffectiveBalance_returns_freeplay_amount_when_freeplay_enabled()
    {
        using var factory = new SVSimTestFactory(freeplayEnabled: true);
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.AsNoTracking().FirstAsync(x => x.Id == viewerId);

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var freeCfg = scope.ServiceProvider.GetRequiredService<IGameConfigService>().Get<SVSim.Database.Models.Config.FreeplayConfig>();
        Assert.That(inv.EffectiveBalance(v, SpendCurrency.Crystal), Is.EqualTo(checked((long)freeCfg.CurrencyAmount)));
    }

    [Test]
    public async Task Transaction_EffectiveBalance_matches_viewer_balance()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 5678;
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        Assert.That(tx.EffectiveBalance(SpendCurrency.Crystal), Is.EqualTo(5678));
    }

    [Test]
    public async Task Transaction_OwnsCard_returns_true_when_card_owned()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId, 1);
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        Assert.That(tx.OwnsCard(cardId), Is.True);
    }

    [Test]
    public async Task Transaction_OwnsCard_returns_false_when_card_not_owned()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        // Do NOT seed owned card
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        Assert.That(tx.OwnsCard(cardId), Is.False);
    }

    [Test]
    public async Task Transaction_OwnsCosmetic_returns_true_when_owned()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_040_000;
        var sleeve = new SleeveEntry { Id = sleeveId };
        ctx.Sleeves.Add(sleeve);
        var v = await ctx.Viewers.Include(x => x.Sleeves).FirstAsync(x => x.Id == viewerId);
        v.Sleeves.Add(sleeve);
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);

        Assert.That(tx.OwnsCosmetic(CosmeticType.Sleeve, sleeveId), Is.True);
    }
}
