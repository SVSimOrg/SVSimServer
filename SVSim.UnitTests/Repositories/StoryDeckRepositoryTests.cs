using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.BuildDeck;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class StoryDeckRepositoryTests
{
    [Test]
    public async Task GetStoryDecksByClass_returns_decks_with_expanded_card_arrays()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // FK: BuildDeckProducts requires a parent BuildDeckSeries row.
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry { Id = 0 });
        await db.SaveChangesAsync();

        // Product 701 (class 1 build): 2x card 100, 1x card 200 = 3-card "deck".
        db.BuildDeckProducts.Add(new BuildDeckProductEntry
        {
            Id = 701, SeriesId = 0, LeaderId = 1, DeckCode = "", ProductNameKey = "", IsEnabled = false,
            Cards = new()
            {
                new BuildDeckProductCardEntry { CardId = 100, Number = 2, IsSpot = false },
                new BuildDeckProductCardEntry { CardId = 200, Number = 1, IsSpot = false },
            },
        });
        db.StoryDecks.Add(new StoryDeckEntry
        {
            DeckNo = 701, Kind = StoryDeckKind.Build, ClassId = 1, DeckName = "Pure Devotion",
            SleeveId = 3000011, LeaderSkinId = 1, IsRecommend = 0, OrderNum = 0, EntryNo = 0, DeckFormat = null,
        });
        // A class-2 deck that must NOT be returned for class 1.
        db.StoryDecks.Add(new StoryDeckEntry { DeckNo = 702, Kind = StoryDeckKind.Build, ClassId = 2, DeckName = "Other" });
        await db.SaveChangesAsync();

        var repo = new BuildDeckRepository(db);
        var result = await repo.GetStoryDecksByClass(1);

        Assert.That(result.Count, Is.EqualTo(1));
        var deck = result[0];
        Assert.That(deck.DeckNo, Is.EqualTo(701));
        Assert.That(deck.DeckName, Is.EqualTo("Pure Devotion"));
        Assert.That(deck.Kind, Is.EqualTo(StoryDeckKind.Build));
        Assert.That(deck.CardIdArray.OrderBy(x => x), Is.EqualTo(new long[] { 100, 100, 200 }));
    }

    [Test]
    public async Task GetStoryDecksByClass_returns_empty_for_class_with_no_decks()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var repo = new BuildDeckRepository(db);
        var result = await repo.GetStoryDecksByClass(8);

        Assert.That(result, Is.Empty);
    }
}
