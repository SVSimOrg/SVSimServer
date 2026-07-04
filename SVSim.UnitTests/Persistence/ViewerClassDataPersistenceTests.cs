using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Persistence;

public class ViewerClassDataPersistenceTests
{
    [Test]
    public async Task ViewerClassData_round_trips_is_random_leader_skin()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await ctx.Viewers
                .Include(v => v.Classes)
                .FirstAsync(v => v.Id == viewerId);
            Assert.That(viewer.Classes, Is.Not.Empty, "fresh viewer should have seeded ViewerClassData rows");
            viewer.Classes[0].IsRandomLeaderSkin = true;
            await ctx.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer2 = await ctx2.Viewers
            .Include(v => v.Classes)
            .AsNoTracking()
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer2.Classes[0].IsRandomLeaderSkin, Is.True, "flag should round-trip through SaveChanges");

        // Other classes still default to false.
        Assert.That(viewer2.Classes.Skip(1).All(c => !c.IsRandomLeaderSkin), Is.True);
    }

    [Test]
    public async Task ViewerClassData_defaults_is_random_leader_skin_to_false()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await ctx.Viewers
            .Include(v => v.Classes)
            .AsNoTracking()
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(viewer.Classes.All(c => !c.IsRandomLeaderSkin), Is.True);
    }
}
