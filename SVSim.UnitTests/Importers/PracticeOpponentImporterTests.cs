using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PracticeOpponentImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_opponents_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PracticeOpponentImporter().ImportAsync(db, SeedDir);

        var opponents = await db.PracticeOpponents.OrderBy(p => p.Id).ToListAsync();
        Assert.That(opponents.Count, Is.GreaterThan(0), "seed file must contain opponents");
        Assert.That(opponents.All(o => o.ClassId >= 0), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PracticeOpponentImporter().ImportAsync(db, SeedDir);
        int before = await db.PracticeOpponents.CountAsync();
        await new PracticeOpponentImporter().ImportAsync(db, SeedDir);
        int after = await db.PracticeOpponents.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Pre-seed an opponent the production seed file doesn't contain.
        const int legacyId = 9999;
        db.PracticeOpponents.Add(new SVSim.Database.Models.PracticeOpponentEntry
        {
            Id = legacyId,
            TextId = "legacy",
            ClassId = 1,
            CharaId = 1,
            DegreeId = 1,
            AiDeckLevel = 1,
            AiLogicLevel = 1,
            AiMaxLife = 20,
            Battle3dFieldId = "1",
        });
        await db.SaveChangesAsync();

        await new PracticeOpponentImporter().ImportAsync(db, SeedDir);

        var legacy = await db.PracticeOpponents.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.TextId, Is.EqualTo("legacy"), "pre-existing values must not be wiped");
    }

    [Test]
    public async Task Skips_rows_with_zero_practice_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Write a one-row seed with PracticeId=0 to a temp dir and confirm it doesn't insert.
        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "practice-opponents.json"),
                "[{\"practice_id\":0,\"text_id\":\"junk\",\"class_id\":1}]");

            await new PracticeOpponentImporter().ImportAsync(db, tmp);

            int count = await db.PracticeOpponents.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with practice_id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }
}
