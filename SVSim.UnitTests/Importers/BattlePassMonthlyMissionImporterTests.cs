using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BattlePassMonthlyMissionImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_may_2026_captured_rows()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);

        int mayCount = await db.BattlePassMonthlyMissions.CountAsync(r => r.Year == 2026 && r.Month == 5);
        Assert.That(mayCount, Is.EqualTo(5), "May 2026 captured 5 monthly mission rows");

        var noRewardRow = await db.BattlePassMonthlyMissions
            .SingleAsync(r => r.Name.StartsWith("Play 5 Challenge"));
        Assert.That(noRewardRow.RewardType, Is.Null, "Play 5 Challenge has no reward_info on wire");
    }

    [Test]
    public async Task Multiple_months_coexist()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.BattlePassMonthlyMissions.Add(new SVSim.Database.Models.BattlePassMonthlyMissionEntry
        {
            Year = 2026, Month = 6, OrderNum = 0,
            Name = "future placeholder", RequireNumber = 1, BattlePassPoint = 100,
            EventType = "ranked_or_arena_win",
        });
        await db.SaveChangesAsync();

        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);

        int mayCount = await db.BattlePassMonthlyMissions.CountAsync(r => r.Year == 2026 && r.Month == 5);
        int juneCount = await db.BattlePassMonthlyMissions.CountAsync(r => r.Year == 2026 && r.Month == 6);
        Assert.That(mayCount, Is.EqualTo(5));
        Assert.That(juneCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);
        int before = await db.BattlePassMonthlyMissions.CountAsync();
        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);
        int after = await db.BattlePassMonthlyMissions.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Empty_seed_is_no_op()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "bp-monthly-missions.json"), "[]");
            await new BattlePassMonthlyMissionImporter().ImportAsync(db, tmp);
            int count = await db.BattlePassMonthlyMissions.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
