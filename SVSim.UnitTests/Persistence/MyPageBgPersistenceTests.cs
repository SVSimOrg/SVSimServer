using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Persistence;

public class MyPageBgPersistenceTests
{
    [Test]
    public async Task Viewer_round_trips_mypage_bg_scalars_and_rotation_pool()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await ctx.Viewers
                .Include(v => v.MyPageBgRotation)
                .FirstAsync(v => v.Id == viewerId);
            viewer.MyPageBgSelectType = 2;
            viewer.MyPageBgId = 1213410310;
            viewer.MyPageBgRotation.Add(new MyPageBgRotationEntry { Slot = 0, BgId = 1211410310 });
            viewer.MyPageBgRotation.Add(new MyPageBgRotationEntry { Slot = 1, BgId = 1212410310 });
            viewer.MyPageBgRotation.Add(new MyPageBgRotationEntry { Slot = 2, BgId = 1213410310 });
            await ctx.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var roundtrip = await ctx2.Viewers
            .Include(v => v.MyPageBgRotation)
            .AsNoTracking()
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(roundtrip.MyPageBgSelectType, Is.EqualTo(2));
        Assert.That(roundtrip.MyPageBgId, Is.EqualTo(1213410310));
        Assert.That(roundtrip.MyPageBgRotation.OrderBy(r => r.Slot).Select(r => r.BgId),
            Is.EqualTo(new[] { 1211410310, 1212410310, 1213410310 }));
    }

    [Test]
    public async Task Viewer_fresh_viewer_has_zero_defaults_and_empty_rotation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await ctx.Viewers
            .Include(v => v.MyPageBgRotation)
            .AsNoTracking()
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(viewer.MyPageBgSelectType, Is.EqualTo(0));
        Assert.That(viewer.MyPageBgId, Is.EqualTo(0));
        Assert.That(viewer.MyPageBgRotation, Is.Empty);
    }
}
