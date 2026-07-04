using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class MissionProgressServiceTests
{
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
    public async Task RecordEvent_increments_all_four_periods()
    {
        using var factory = new SVSimTestFactory();
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IMissionProgressService>();
        await svc.RecordEventAsync(vid, new[] { "ranked_win" });

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var counters = await db.ViewerEventCounters
            .Where(c => c.ViewerId == vid && c.EventKey == "ranked_win").ToListAsync();
        Assert.That(counters.Count, Is.EqualTo(4),
            "expect day + week + month + all-time rows");
        Assert.That(counters.All(c => c.Count == 1));
        Assert.That(counters.Select(c => c.Period.Split(':')[0]).OrderBy(s => s),
            Is.EquivalentTo(new[] { "all-time", "day", "month", "week" }));
    }

    [Test]
    public async Task RecordEvent_with_multiple_keys_increments_each_independently()
    {
        using var factory = new SVSimTestFactory();
        long vid = await SeedViewer(factory.Services);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IMissionProgressService>();
        await svc.RecordEventAsync(vid, new[] { "ranked_win", "ranked_win:swordcraft" });

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        int broad = await db.ViewerEventCounters.CountAsync(
            c => c.ViewerId == vid && c.EventKey == "ranked_win");
        int narrow = await db.ViewerEventCounters.CountAsync(
            c => c.ViewerId == vid && c.EventKey == "ranked_win:swordcraft");
        Assert.That(broad, Is.EqualTo(4));
        Assert.That(narrow, Is.EqualTo(4));
    }

    [Test]
    public async Task RecordEvent_increments_existing_counter()
    {
        using var factory = new SVSimTestFactory();
        long vid = await SeedViewer(factory.Services);

        using (var scope1 = factory.Services.CreateScope())
        {
            var svc = scope1.ServiceProvider.GetRequiredService<IMissionProgressService>();
            await svc.RecordEventAsync(vid, new[] { "ranked_win" });
        }
        using (var scope2 = factory.Services.CreateScope())
        {
            var svc = scope2.ServiceProvider.GetRequiredService<IMissionProgressService>();
            await svc.RecordEventAsync(vid, new[] { "ranked_win" });
        }

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var allTime = await db.ViewerEventCounters.FirstAsync(
            c => c.ViewerId == vid && c.EventKey == "ranked_win" && c.Period == "all-time");
        Assert.That(allTime.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task RecordEvent_marks_achievement_claimable_when_threshold_hit()
    {
        using var factory = new SVSimTestFactory();
        long vid = await SeedViewer(factory.Services);

        using (var setupScope = factory.Services.CreateScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            // Seed the catalog row the service will look up. (Test factory doesn't run importers.)
            db.AchievementCatalog.Add(new AchievementCatalogEntry
            {
                AchievementType = 31, Level = 3, Name = "Win 50 ranked matches",
                RequireNumber = 50, RewardType = (UserGoodsType)9, RewardDetailId = 0, RewardNumber = 20,
                OrderNum = 18, EventType = "ranked_win", EventArg = null,
            });
            // Set viewer's current level to 3.
            db.ViewerAchievements.Add(new ViewerAchievement
            {
                ViewerId = vid, AchievementType = 31, Level = 3, AchievementStatus = 0,
                NowAchievedLevel = 0, ResultAnnounceSawLevel = 0,
            });
            // Pre-set 49 wins so one more hits threshold.
            db.ViewerEventCounters.Add(new ViewerEventCounter
            {
                ViewerId = vid, EventKey = "ranked_win", Period = "all-time", Count = 49,
            });
            await db.SaveChangesAsync();
        }

        using (var actScope = factory.Services.CreateScope())
        {
            var svc = actScope.ServiceProvider.GetRequiredService<IMissionProgressService>();
            await svc.RecordEventAsync(vid, new[] { "ranked_win" });
        }

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var refreshed = await verifyDb.ViewerAchievements.FirstAsync(
            a => a.ViewerId == vid && a.AchievementType == 31);
        Assert.That(refreshed.AchievementStatus, Is.EqualTo(1), "should be claimable after threshold");
    }
}
