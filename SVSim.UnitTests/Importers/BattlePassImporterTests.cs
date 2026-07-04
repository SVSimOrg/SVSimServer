using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BattlePassImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_level_curve_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassImporter().ImportAsync(db, SeedDir);

        var levels = await db.BattlePassLevels.OrderBy(e => e.Level).ToListAsync();
        Assert.That(levels.Count, Is.EqualTo(100), "seed must contain 100 levels");
        Assert.That(levels[0].Level, Is.EqualTo(1));
        Assert.That(levels[0].RequiredPoint, Is.EqualTo(0));
        Assert.That(levels[1].Level, Is.EqualTo(2));
        Assert.That(levels[1].RequiredPoint, Is.EqualTo(500));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassImporter().ImportAsync(db, SeedDir);
        int before = await db.BattlePassLevels.CountAsync();
        await new BattlePassImporter().ImportAsync(db, SeedDir);
        int after = await db.BattlePassLevels.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.BattlePassLevels.Add(new SVSim.Database.Models.BattlePassLevelEntry
        {
            Level = 999, RequiredPoint = 12345,
        });
        await db.SaveChangesAsync();

        await new BattlePassImporter().ImportAsync(db, SeedDir);

        var legacy = await db.BattlePassLevels.FindAsync(999);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.RequiredPoint, Is.EqualTo(12345));
    }

    [Test]
    public async Task Empty_seed_file_is_no_op()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "battle-pass-levels.json"), "[]");
            await new BattlePassImporter().ImportAsync(db, tmp);
            int count = await db.BattlePassLevels.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
