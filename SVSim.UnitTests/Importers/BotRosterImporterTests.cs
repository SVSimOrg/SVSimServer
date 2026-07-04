using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BotRosterImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_bots_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BotRosterImporter().ImportAsync(db, SeedDir);

        var bots = await db.BotRoster.OrderBy(b => b.Id).ToListAsync();
        Assert.That(bots.Count, Is.GreaterThan(0), "seed file must contain bots");
        Assert.That(bots.All(b => b.ClassId is >= 1 and <= 8), Is.True);
        Assert.That(bots.All(b => !string.IsNullOrEmpty(b.UserName)), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BotRosterImporter().ImportAsync(db, SeedDir);
        int before = await db.BotRoster.CountAsync();
        await new BotRosterImporter().ImportAsync(db, SeedDir);
        int after = await db.BotRoster.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyAiId = 99999;
        db.BotRoster.Add(new SVSim.Database.Models.BotRosterEntry
        {
            Id = legacyAiId,
            CountryCode = "ZZ",
            UserName = "legacy",
            SleeveId = 1,
            EmblemId = 1,
            DegreeId = 1,
            FieldId = 1,
            ClassId = 1,
            CharaId = 1,
            Rank = 1,
        });
        await db.SaveChangesAsync();

        await new BotRosterImporter().ImportAsync(db, SeedDir);

        var legacy = await db.BotRoster.FindAsync(legacyAiId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.UserName, Is.EqualTo("legacy"), "pre-existing values must not be wiped");
    }

    [Test]
    public async Task Skips_rows_with_zero_ai_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "bot-roster.json"),
                "[{\"ai_id\":0,\"user_name\":\"junk\",\"class_id\":1}]");

            await new BotRosterImporter().ImportAsync(db, tmp);

            int count = await db.BotRoster.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with ai_id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }
}
