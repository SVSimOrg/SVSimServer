using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class StoryDeckImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_story_decks_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new StoryDeckImporter().ImportAsync(db, SeedDir);

        var decks = await db.StoryDecks.OrderBy(d => d.Id).ToListAsync();
        Assert.That(decks.Count, Is.EqualTo(112), "53 build + 59 trial");
        Assert.That(decks.Count(d => d.Kind == StoryDeckKind.Build), Is.EqualTo(53));
        Assert.That(decks.Count(d => d.Kind == StoryDeckKind.Trial), Is.EqualTo(59));
        var pureDevotion = decks.Single(d => d.DeckNo == 701);
        Assert.That(pureDevotion.Kind, Is.EqualTo(StoryDeckKind.Build));
        Assert.That(pureDevotion.ClassId, Is.EqualTo(1));
        Assert.That(pureDevotion.DeckName, Is.EqualTo("Pure Devotion"));
        Assert.That(pureDevotion.DeckFormat, Is.Null);
        Assert.That(decks.Where(d => d.Kind == StoryDeckKind.Trial).All(d => d.DeckFormat != null), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new StoryDeckImporter().ImportAsync(db, SeedDir);
        int before = await db.StoryDecks.CountAsync();
        await new StoryDeckImporter().ImportAsync(db, SeedDir);
        int after = await db.StoryDecks.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }
}
