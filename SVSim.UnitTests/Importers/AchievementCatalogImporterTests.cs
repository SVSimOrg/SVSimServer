using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class AchievementCatalogImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_captured_achievement_tiers_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);

        var swordLevel = await db.AchievementCatalog
            .Where(r => r.AchievementType == 12)
            .OrderBy(r => r.Level).LastOrDefaultAsync();
        Assert.That(swordLevel, Is.Not.Null);
        Assert.That(swordLevel!.EventType, Is.EqualTo("class_level_up:swordcraft"));
    }

    [Test]
    public async Task Imports_multiple_levels_for_same_type()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);

        // Hand-add a tier not present in the seed (use a high level the seed won't contain).
        db.AchievementCatalog.Add(new SVSim.Database.Models.AchievementCatalogEntry
        {
            AchievementType = 12, Level = 99,
            Name = "Reach level 999 in Swordcraft (synthetic)", RequireNumber = 999,
            RewardType = (UserGoodsType)5, RewardDetailId = 100211062, RewardNumber = 3, OrderNum = 10,
            EventType = "class_level_up:swordcraft", EventArg = null,
        });
        await db.SaveChangesAsync();

        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);

        int swordTiers = await db.AchievementCatalog.CountAsync(r => r.AchievementType == 12);
        Assert.That(swordTiers, Is.GreaterThanOrEqualTo(3),
            "hand-added tier 99 must coexist with seeded tiers 6 and 7");
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);
        int before = await db.AchievementCatalog.CountAsync();
        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);
        int after = await db.AchievementCatalog.CountAsync();

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
            File.WriteAllText(Path.Combine(tmp, "achievement-catalog.json"), "[]");
            await new AchievementCatalogImporter().ImportAsync(db, tmp);
            int count = await db.AchievementCatalog.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
