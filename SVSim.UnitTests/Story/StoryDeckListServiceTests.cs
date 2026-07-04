using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Story;

[TestFixture]
public class StoryDeckListServiceTests
{
    [Test]
    public async Task GetDeckList_populates_build_trial_and_default_for_chapter_class()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // StoryChapter.SectionId has an enforced FK to StorySection; seed the parent row first.
        db.StorySections.Add(new StorySection { Id = 1, StoryApiType = StoryApiType.Main });
        // Chapter 14 is a class-1 (Forestcraft) chapter.
        db.StoryChapters.Add(new StoryChapter { StoryId = 14, SectionId = 1, CharaId = 1, ChapterId = "14" });

        // One class-1 build deck (701) + one class-1 trial deck (13001), each with a 1-card product.
        // BuildDeckProductEntry has an enforced FK SeriesId -> BuildDeckSeries; seed the parent first.
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry { Id = 0 });
        db.BuildDeckProducts.Add(new BuildDeckProductEntry { Id = 701, Cards = new() { new BuildDeckProductCardEntry { CardId = 100, Number = 1 } } });
        db.BuildDeckProducts.Add(new BuildDeckProductEntry { Id = 13001, Cards = new() { new BuildDeckProductCardEntry { CardId = 200, Number = 1 } } });
        db.StoryDecks.Add(new StoryDeckEntry { DeckNo = 701, Kind = StoryDeckKind.Build, ClassId = 1, DeckName = "Pure Devotion", DeckFormat = null });
        db.StoryDecks.Add(new StoryDeckEntry { DeckNo = 13001, Kind = StoryDeckKind.Trial, ClassId = 1, DeckName = "Tempo Forestcraft", DeckFormat = 1 });

        db.DefaultDecks.Add(new DefaultDeckEntry { Id = 91, ClassId = 1, SleeveId = 3000011, LeaderSkinId = 0, DeckName = "Default", CardIdArray = "[100,100,100]" });
        await db.SaveChangesAsync();

        var service = scope.ServiceProvider.GetRequiredService<IStoryService>();
        var resp = await service.GetDeckListAsync(StoryApiType.Main, storyId: 14, viewerId: 1);

        Assert.That(resp.BuildDeckList.Count, Is.EqualTo(1));
        Assert.That(resp.BuildDeckList[0].DeckNo, Is.EqualTo(701));
        Assert.That(resp.BuildDeckList[0].DeckName, Is.EqualTo("Pure Devotion"));
        Assert.That(resp.BuildDeckList[0].CardIdArray, Is.EqualTo(new long[] { 100 }));

        Assert.That(resp.TrialDeckList.Count, Is.EqualTo(1));
        Assert.That(resp.TrialDeckList[0].DeckNo, Is.EqualTo(13001));
        Assert.That(resp.TrialDeckList[0].DeckFormat, Is.EqualTo(1));

        Assert.That(resp.DefaultDeckList.ContainsKey("91"), Is.True);
        Assert.That(resp.DefaultDeckList["91"].CardIdArray, Is.EqualTo(new long[] { 100, 100, 100 }));
    }

    [Test]
    public async Task GetDeckList_returns_empty_build_trial_for_non_class_chapter()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // StoryChapter.SectionId has an enforced FK to StorySection; seed the parent row first.
        db.StorySections.Add(new StorySection { Id = 17, StoryApiType = StoryApiType.Main });
        // chara_id 0 -> custom-leader / non-class chapter.
        db.StoryChapters.Add(new StoryChapter { StoryId = 500, SectionId = 17, CharaId = 0, ChapterId = "500" });
        await db.SaveChangesAsync();

        var service = scope.ServiceProvider.GetRequiredService<IStoryService>();
        var resp = await service.GetDeckListAsync(StoryApiType.Main, storyId: 500, viewerId: 1);

        Assert.That(resp.BuildDeckList, Is.Empty);
        Assert.That(resp.TrialDeckList, Is.Empty);
    }
}
