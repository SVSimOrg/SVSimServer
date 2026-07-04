using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryLifecycleTests
{
    [Test]
    public async Task Dispose_without_commit_does_not_persist()
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
            // no commit; dispose runs
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v2 = await ctx2.Viewers.AsNoTracking().FirstAsync(x => x.Id == viewerId);
        Assert.That(v2.Currency.Rupees, Is.EqualTo(100UL), "no persistence without commit");
    }

    [Test]
    public async Task Use_after_commit_throws()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        await using var tx = await inv.BeginAsync(viewerId);
        await tx.CommitAsync();

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => { await tx.GrantAsync(UserGoodsType.Rupy, 0, 1); });
    }
}
