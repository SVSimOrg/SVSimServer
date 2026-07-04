using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class DefaultDeckImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_default_decks_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new DefaultDeckImporter().ImportAsync(db, SeedDir);

        var decks = await db.DefaultDecks.OrderBy(d => d.Id).ToListAsync();
        Assert.That(decks.Count, Is.GreaterThan(0), "seed file must contain default decks");
        Assert.That(decks.All(d => d.ClassId > 0), Is.True);
        Assert.That(decks.All(d => !string.IsNullOrEmpty(d.DeckName)), Is.True);
        // CardIdArray is a JSON array column; every row must serialize as such.
        Assert.That(decks.All(d => d.CardIdArray.StartsWith("[")), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new DefaultDeckImporter().ImportAsync(db, SeedDir);
        int before = await db.DefaultDecks.CountAsync();
        await new DefaultDeckImporter().ImportAsync(db, SeedDir);
        int after = await db.DefaultDecks.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 99999;
        db.DefaultDecks.Add(new DefaultDeckEntry
        {
            Id = legacyId,
            ClassId = 1,
            SleeveId = 0,
            LeaderSkinId = 0,
            DeckName = "legacy",
            CardIdArray = "[]",
        });
        await db.SaveChangesAsync();

        await new DefaultDeckImporter().ImportAsync(db, SeedDir);

        var legacy = await db.DefaultDecks.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.DeckName, Is.EqualTo("legacy"));
    }

    [Test]
    public async Task Skips_rows_with_zero_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "default-decks.json"),
                "[{\"id\":0,\"class_id\":1,\"sleeve_id\":0,\"leader_skin_id\":0,\"deck_name\":\"junk\",\"card_id_array\":[1,2,3]}]");

            await new DefaultDeckImporter().ImportAsync(db, tmp);

            int count = await db.DefaultDecks.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }

    [Test]
    public async Task Warns_on_orphan_card_ids_but_does_not_fail()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // The test factory's minimal seed contains only cards 10001001/10001002/10001003.
        // Reference a card id well outside that set so the orphan-count branch fires.
        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "default-decks.json"),
                "[{\"id\":1234,\"class_id\":1,\"sleeve_id\":0,\"leader_skin_id\":0,\"deck_name\":\"orphans\",\"card_id_array\":[999999999,888888888]}]");

            Assert.DoesNotThrowAsync(async () =>
                await new DefaultDeckImporter().ImportAsync(db, tmp),
                "orphan card_ids must warn, never throw");

            var row = await db.DefaultDecks.FindAsync(1234);
            Assert.That(row, Is.Not.Null, "deck must be inserted even with orphan card refs");
            Assert.That(row!.DeckName, Is.EqualTo("orphans"));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
