using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class ViewerMissionStateServiceTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    private static async Task ImportCatalogsAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await new MissionCatalogImporter().ImportAsync(db, SeedDir);
        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);
    }

    private static async Task<long> SeedViewer(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = new Viewer { DisplayName = "test", ShortUdid = 1, LastLogin = DateTime.UtcNow };
        db.Viewers.Add(v);
        await db.SaveChangesAsync();
        return v.Id;
    }

    [Test]
    public async Task EnsureCurrent_creates_one_achievement_per_catalog_type()
    {
        using var factory = new SVSimTestFactory();
        await ImportCatalogsAsync(factory.Services);
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(x => x.Achievements).Include(x => x.Missions)
            .FirstAsync(x => x.Id == vid);

        var svc = scope.ServiceProvider.GetRequiredService<IViewerMissionStateService>();
        await svc.EnsureCurrentAsync(viewer.Id);
        await db.SaveChangesAsync();

        int catalogTypeCount = await db.AchievementCatalog
            .Select(e => e.AchievementType).Distinct().CountAsync();
        int viewerCount = await db.ViewerAchievements.CountAsync(a => a.ViewerId == vid);
        Assert.That(viewerCount, Is.EqualTo(catalogTypeCount));
    }

    [Test]
    public async Task EnsureCurrent_is_idempotent()
    {
        using var factory = new SVSimTestFactory();
        await ImportCatalogsAsync(factory.Services);
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(x => x.Achievements).Include(x => x.Missions)
            .FirstAsync(x => x.Id == vid);

        var svc = scope.ServiceProvider.GetRequiredService<IViewerMissionStateService>();
        await svc.EnsureCurrentAsync(viewer.Id);
        await db.SaveChangesAsync();
        int after1 = await db.ViewerAchievements.CountAsync(a => a.ViewerId == vid);
        await svc.EnsureCurrentAsync(viewer.Id);
        await db.SaveChangesAsync();
        int after2 = await db.ViewerAchievements.CountAsync(a => a.ViewerId == vid);
        Assert.That(after2, Is.EqualTo(after1));
    }

    [Test]
    public async Task EnsureCurrent_assigns_daily_and_weekly_slots_from_pool()
    {
        using var factory = new SVSimTestFactory();
        await ImportCatalogsAsync(factory.Services);
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(x => x.Missions).Include(x => x.Achievements)
            .FirstAsync(x => x.Id == vid);

        var svc = scope.ServiceProvider.GetRequiredService<IViewerMissionStateService>();
        await svc.EnsureCurrentAsync(viewer.Id);
        await db.SaveChangesAsync();

        var slots = await db.ViewerMissions
            .Where(m => m.ViewerId == vid).OrderBy(m => m.Slot).ToListAsync();
        Assert.That(slots.Count, Is.EqualTo(4), "1 daily + 3 weekly");
        Assert.That(slots.Select(s => s.Slot), Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        var dailyCatalogId = slots[0].MissionCatalogId;
        var dailyCatalog = await db.MissionCatalog.FindAsync(dailyCatalogId);
        Assert.That(dailyCatalog!.LotType, Is.EqualTo(6), "slot 0 = daily, lot_type 6");
    }

    [Test]
    public async Task EnsureCurrent_picks_distinct_weekly_missions()
    {
        using var factory = new SVSimTestFactory();
        await ImportCatalogsAsync(factory.Services);
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(x => x.Missions).Include(x => x.Achievements)
            .FirstAsync(x => x.Id == vid);

        var svc = scope.ServiceProvider.GetRequiredService<IViewerMissionStateService>();
        await svc.EnsureCurrentAsync(viewer.Id);
        await db.SaveChangesAsync();

        var weeklyIds = await db.ViewerMissions
            .Where(m => m.ViewerId == vid && m.Slot != 0)
            .Select(m => m.MissionCatalogId).ToListAsync();
        Assert.That(weeklyIds.Distinct().Count(), Is.EqualTo(weeklyIds.Count),
            "weekly slots must have distinct catalog ids");
    }
}
