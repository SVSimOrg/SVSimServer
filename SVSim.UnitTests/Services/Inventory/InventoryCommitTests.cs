using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryCommitTests
{
    [Test]
    public async Task Commit_emits_one_currency_entry_with_grant_post_state_when_spend_then_grant()
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
        await tx.TrySpendAsync(SpendCurrency.Crystal, 500);
        await tx.GrantAsync(UserGoodsType.Crystal, 0, 200);
        var result = await tx.CommitAsync();

        var crystals = result.RewardList.Where(r => r.RewardType == UserGoodsType.Crystal).ToList();
        Assert.That(crystals, Has.Count.EqualTo(1));
        Assert.That(crystals[0].RewardNum, Is.EqualTo(700), "spend 500 then grant 200 → 1000-500+200=700, grant's post-state wins");
    }

    [Test]
    public async Task Commit_emits_one_currency_entry_with_spend_post_state_when_grant_then_spend()
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
        await tx.GrantAsync(UserGoodsType.Crystal, 0, 200);
        await tx.TrySpendAsync(SpendCurrency.Crystal, 500);
        var result = await tx.CommitAsync();

        var crystals = result.RewardList.Where(r => r.RewardType == UserGoodsType.Crystal).ToList();
        Assert.That(crystals, Has.Count.EqualTo(1));
        Assert.That(crystals[0].RewardNum, Is.EqualTo(700));
    }

    [Test]
    public async Task Commit_persists_mutations()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Rupees = 100;
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using (var tx = await inv.BeginAsync(viewerId))
        {
            await tx.GrantAsync(UserGoodsType.Rupy, 0, 50);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v2 = await ctx2.Viewers.AsNoTracking().FirstAsync(x => x.Id == viewerId);
        Assert.That(v2.Currency.Rupees, Is.EqualTo(150UL));
    }

    [Test]
    public async Task Deltas_are_verbatim_queued_no_cascade()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        const int sleeveId = 2_000_030_000;
        ctx.Sleeves.Add(new SVSim.Database.Models.SleeveEntry { Id = sleeveId });
        ctx.CardCosmeticRewards.Add(new SVSim.Database.Models.CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        await tx.GrantAsync(UserGoodsType.Card, cardId, 1);
        var result = await tx.CommitAsync();

        Assert.That(result.Deltas, Has.Count.EqualTo(1), "verbatim — card only, no cascade");
        Assert.That(result.Deltas[0].RewardType, Is.EqualTo(UserGoodsType.Card));
        Assert.That(result.RewardList.Any(e => e.RewardType == UserGoodsType.Sleeve), Is.True,
            "cascade appears in RewardList but not Deltas");
    }
}
