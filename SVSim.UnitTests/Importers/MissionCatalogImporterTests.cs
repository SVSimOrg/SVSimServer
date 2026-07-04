using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class MissionCatalogImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_captured_missions_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new MissionCatalogImporter().ImportAsync(db, SeedDir);

        var rows = await db.MissionCatalog.OrderBy(e => e.Id).ToListAsync();
        Assert.That(rows.Count, Is.GreaterThanOrEqualTo(5),
            "captured mission_ids 12, 16, 20, 332, 505 must all be present");

        var daily = rows.Single(r => r.Id == 332);
        Assert.That(daily.LotType, Is.EqualTo(6));
        Assert.That(daily.DefaultFlag, Is.True);
        Assert.That(daily.EventType, Is.EqualTo("daily_match_win"));

        var sword = rows.Single(r => r.Id == 16);
        Assert.That(sword.LotType, Is.EqualTo(2));
        Assert.That(sword.EventType, Is.EqualTo("ranked_win:swordcraft"));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new MissionCatalogImporter().ImportAsync(db, SeedDir);
        int before = await db.MissionCatalog.CountAsync();
        await new MissionCatalogImporter().ImportAsync(db, SeedDir);
        int after = await db.MissionCatalog.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.MissionCatalog.Add(new SVSim.Database.Models.MissionCatalogEntry
        {
            Id = 99999, Name = "preserved", LotType = 2,
            RequireNumber = 1, RewardType = (UserGoodsType)9, RewardDetailId = 0, RewardNumber = 10,
            BattlePassPoint = 0, DefaultFlag = false, StartTime = 0,
        });
        await db.SaveChangesAsync();

        await new MissionCatalogImporter().ImportAsync(db, SeedDir);

        var preserved = await db.MissionCatalog.FindAsync(99999);
        Assert.That(preserved, Is.Not.Null);
        Assert.That(preserved!.Name, Is.EqualTo("preserved"));
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
            File.WriteAllText(Path.Combine(tmp, "mission-catalog.json"), "[]");
            await new MissionCatalogImporter().ImportAsync(db, tmp);
            int count = await db.MissionCatalog.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
