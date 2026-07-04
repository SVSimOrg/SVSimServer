using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Story;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Story;

[TestFixture]
// One instance per test case so parallel tests don't race on the SetUp-initialised
// _master / _viewer / _service fields. NUnit's default SingleInstance shares the
// fixture instance across all tests in the class; under ParallelScope.All, concurrent
// SetUps wipe each other's Mock setups and we see NullReferenceExceptions in service code.
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class StoryServiceTests
{
    private Mock<IStoryMasterRepository> _master = null!;
    private Mock<IViewerStoryProgressRepository> _viewer = null!;
    private StoryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _master = new Mock<IStoryMasterRepository>();
        _viewer = new Mock<IViewerStoryProgressRepository>();
        // Non-reward tests never exercise the DB/reward path; use a stub InMemory context + null inv.
        var db = StoryServiceTestHelpers.NewInMemoryDb(nameof(SetUp));
        var inv = new Mock<IInventoryService>().Object;
        _service = new StoryService(
            _master.Object, _viewer.Object,
            inv: inv,
            db: db,
            configService: StoryServiceTestHelpers.NewConfigService(),
            deckRepository: new Mock<SVSim.Database.Repositories.Deck.IDeckRepository>().Object,
            buildDecks: new Mock<SVSim.Database.Repositories.BuildDeck.IBuildDeckRepository>().Object,
            viewers: new Mock<SVSim.Database.Repositories.Viewer.IViewerRepository>().Object,
            xp: new Mock<SVSim.Database.Services.BattleXp.IBattleXpService>().Object,
            logger: NullLogger<StoryService>.Instance);
    }

    /// <summary>
    /// Creates a <see cref="StoryService"/> backed by a real <see cref="SVSimDbContext"/> from
    /// <paramref name="factory"/>, seeds a viewer with RedEther reset to 0, and returns the
    /// service + viewer's actual ID.
    /// The caller owns the factory lifetime; keep it alive for post-call assertions.
    /// </summary>
    private StoryService NewServiceWithSeededViewer(
        SVSimTestFactory factory,
        out IServiceScope scope,
        out long viewerId)
    {
        viewerId = factory.SeedViewerAsync().GetAwaiter().GetResult();

        // Reset RedEther to 0 so tests can assert literal post-state totals (spec requirement).
        var seedId = viewerId;
        using (var resetScope = factory.Services.CreateScope())
        {
            var resetDb = resetScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = resetDb.Viewers.First(x => x.Id == seedId);
            v.Currency.RedEther = 0;
            resetDb.SaveChanges();
        }

        scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        return new StoryService(
            _master.Object,
            _viewer.Object,
            inv: inv,
            db: db,
            configService: StoryServiceTestHelpers.NewConfigService(),
            deckRepository: new Mock<SVSim.Database.Repositories.Deck.IDeckRepository>().Object,
            buildDecks: new Mock<SVSim.Database.Repositories.BuildDeck.IBuildDeckRepository>().Object,
            viewers: scope.ServiceProvider.GetRequiredService<SVSim.Database.Repositories.Viewer.IViewerRepository>(),
            xp: scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.BattleXp.IBattleXpService>(),
            logger: NullLogger<StoryService>.Instance);
    }

    private static StoryChapter Ch(int storyId, int section, int chara, string chapId, string nextId,
        bool battle = true, int? sbsId = null) =>
        new() { StoryId = storyId, SectionId = section, CharaId = chara,
                ChapterId = chapId, NextChapterId = nextId, BattleExists = battle,
                SpecialBattleSettingId = sbsId, IsSkipEnabled = true };

    [Test]
    public async Task GetInfoAsync_chapter1_always_released_chapter2_locked_when_no_progress()
    {
        var chapters = new List<StoryChapter> {
            Ch(100, 1, 2, "1", "2"),
            Ch(101, 1, 2, "2", "3"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(1, 2)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 1, 2, viewerId: 7L);

        var ch1 = resp.StoryMasterList.Single(c => c.ChapterId == "1");
        var ch2 = resp.StoryMasterList.Single(c => c.ChapterId == "2");
        Assert.That(ch1.IsReleased, Is.True);
        Assert.That(ch2.IsReleased, Is.False);
    }

    [Test]
    public async Task GetInfoAsync_chapter2_released_after_chapter1_finished()
    {
        var chapters = new List<StoryChapter> {
            Ch(100, 1, 2, "1", "2"),
            Ch(101, 1, 2, "2", "3"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(1, 2)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 100, new ViewerStoryProgress { ViewerId = 7, StoryId = 100, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 1, 2, viewerId: 7L);

        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "2").IsReleased, Is.True);
    }

    [Test]
    public async Task GetInfoAsync_chapter2_released_after_skip_clear_too()
    {
        var chapters = new List<StoryChapter> {
            Ch(100, 1, 2, "1", "2"), Ch(101, 1, 2, "2", "3"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(1, 2)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 100, new ViewerStoryProgress { StoryId = 100, IsSkipped = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 1, 2, viewerId: 7L);
        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "2").IsReleased, Is.True);
    }

    [Test]
    public async Task GetInfoAsync_branch_children_locked_without_explicit_unlock()
    {
        var chapters = new List<StoryChapter> {
            Ch(200, 17, 500901, "2", "3a 3b 3c"),
            Ch(201, 17, 500901, "3a", "4a"),
            Ch(202, 17, 500901, "3b", "4b"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 200, new ViewerStoryProgress { StoryId = 200, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 17, 500901, viewerId: 7L);

        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "3a").IsLock, Is.True);
        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "3b").IsLock, Is.True);
    }

    [Test]
    public async Task GetInfoAsync_branch_child_released_when_unlock_exists()
    {
        var chapters = new List<StoryChapter> {
            Ch(200, 17, 500901, "2", "3a 3b 3c"),
            Ch(201, 17, 500901, "3a", "4a"),
            Ch(202, 17, 500901, "3b", "4b"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 200, new ViewerStoryProgress { StoryId = 200, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int> { 201 });   // only 3a unlocked

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 17, 500901, viewerId: 7L);

        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "3a").IsLock, Is.False, "selected branch is playable");
        Assert.That(resp.StoryMasterList.Single(c => c.ChapterId == "3b").IsLock, Is.True, "unselected branch is locked");
    }

    [Test]
    public async Task GetInfoAsync_branch_siblings_stay_visible_after_parent_finished_even_when_locked()
    {
        // Section 17 chara 500901 (Havencraft): ch2's selection_chapter_id picks one of 3a/3b/3c.
        // The two NOT chosen must stay visible (is_released=true) with is_lock=true so the UI
        // can render them as "locked alternative branches" — they vanish entirely if we tie
        // is_released to is_lock. Verified against traffic_prod_haven_choices.ndjson lines 22,28,34
        // (post-clear state showing chosen branch unlocked, others released-but-locked).
        var chapters = new List<StoryChapter> {
            Ch(200, 17, 500901, "2", "3a 3b 3c"),
            Ch(201, 17, 500901, "3a", "4a"),
            Ch(202, 17, 500901, "3b", "4b"),
            Ch(203, 17, 500901, "3c", "4c"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 200, new ViewerStoryProgress { StoryId = 200, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int> { 201 });   // user picked 3a

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 17, 500901, viewerId: 7L);

        var c3a = resp.StoryMasterList.Single(c => c.ChapterId == "3a");
        var c3b = resp.StoryMasterList.Single(c => c.ChapterId == "3b");
        var c3c = resp.StoryMasterList.Single(c => c.ChapterId == "3c");

        Assert.That(c3a.IsReleased, Is.True);  Assert.That(c3a.IsLock, Is.False);   // selected
        Assert.That(c3b.IsReleased, Is.True);  Assert.That(c3b.IsLock, Is.True);    // visible-but-locked
        Assert.That(c3c.IsReleased, Is.True);  Assert.That(c3c.IsLock, Is.True);    // visible-but-locked
    }

    [Test]
    public async Task GetInfoAsync_emits_unlock_text_from_chapter_master()
    {
        // Client renders "Complete the following requirements to unlock this story: {0}" and
        // substitutes {0} with unlock_text. An empty unlock_text leaves the literal "{0}" visible.
        // Verified against traffic_prod_haven_choices.ndjson where every branch sibling carries
        // a populated unlock_text (e.g. "Select 'Head to the West Tower' in Chapter 2").
        var parent = Ch(200, 17, 500901, "2", "3a 3b");
        var branch3b = Ch(202, 17, 500901, "3b", "4b");
        branch3b.UnlockText = "Select \"Look for Leads on Amaryllis\" in Chapter 2";
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901))
               .ReturnsAsync(new List<StoryChapter> { parent, branch3b });
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 200, new ViewerStoryProgress { StoryId = 200, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 17, 500901, viewerId: 7L);

        var c3b = resp.StoryMasterList.Single(c => c.ChapterId == "3b");
        Assert.That(c3b.IsLock, Is.True, "precondition: chapter is locked");
        Assert.That(c3b.UnlockText, Is.EqualTo("Select \"Look for Leads on Amaryllis\" in Chapter 2"));
    }

    [Test]
    public async Task GetInfoAsync_non_branch_downstream_of_unfinished_branch_is_unreleased_but_unlocked()
    {
        // Prod traffic_prod_haven_choices line 40: after ch3a is finished, ch4a is released+playable
        // but ch4b/ch4c are NOT released yet (their parent ch3b/3c not finished) AND is_lock=false
        // — is_lock is reserved for the branch-sibling gate, not the inverse of is_released.
        var chapters = new List<StoryChapter> {
            Ch(200, 17, 500901, "2", "3a 3b 3c"),
            Ch(201, 17, 500901, "3a", "4a"),
            Ch(202, 17, 500901, "3b", "4b"),
            Ch(203, 17, 500901, "3c", "4c"),
            Ch(300, 17, 500901, "4a", "5a"),
            Ch(301, 17, 500901, "4b", "5b"),
            Ch(302, 17, 500901, "4c", "7"),
        };
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901)).ReturnsAsync(chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 200, new ViewerStoryProgress { StoryId = 200, IsFinish = true } },
                   { 201, new ViewerStoryProgress { StoryId = 201, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int> { 201 });   // user picked + finished 3a

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 17, 500901, viewerId: 7L);

        var c4a = resp.StoryMasterList.Single(c => c.ChapterId == "4a");
        var c4b = resp.StoryMasterList.Single(c => c.ChapterId == "4b");
        Assert.That(c4a.IsReleased, Is.True);   Assert.That(c4a.IsLock, Is.False);  // playable
        Assert.That(c4b.IsReleased, Is.False);  Assert.That(c4b.IsLock, Is.False);  // not reached, but not "locked"
    }

    [Test]
    public async Task GetLeaderSelectAsync_untouched_chara_has_current_chapter_1()
    {
        var allChapters = new[] { 1, 2, 3, 4, 5, 6, 7, 8 }
            .Select(chara => Ch(100 + chara, 1, chara, "1", "2")).ToList();
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1))))
               .ReturnsAsync(allChapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var resp = await _service.GetLeaderSelectAsync(StoryApiType.Main, 1, viewerId: 7L);

        Assert.That(resp.LeaderList, Has.Count.EqualTo(8));
        Assert.That(resp.LeaderList.All(l => l.CurrentChapter == 1));
        Assert.That(resp.LeaderCount, Is.EqualTo(8));
    }

    [Test]
    public async Task GetLeaderSelectAsync_after_clearing_chapter5_current_chapter_is_6()
    {
        // Section only seeded with chara=2 here — new impl returns only chara_ids actually present.
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1))))
               .ReturnsAsync(new List<StoryChapter> {
                   Ch(101, 1, 2, "1", "2"), Ch(102, 1, 2, "2", "3"), Ch(103, 1, 2, "3", "4"),
                   Ch(104, 1, 2, "4", "5"), Ch(105, 1, 2, "5", "6"), Ch(106, 1, 2, "6", "0"),
               });
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 101, new ViewerStoryProgress { StoryId = 101, IsFinish = true } },
                   { 102, new ViewerStoryProgress { StoryId = 102, IsFinish = true } },
                   { 103, new ViewerStoryProgress { StoryId = 103, IsFinish = true } },
                   { 104, new ViewerStoryProgress { StoryId = 104, IsFinish = true } },
                   { 105, new ViewerStoryProgress { StoryId = 105, IsFinish = true } },
               });

        var resp = await _service.GetLeaderSelectAsync(StoryApiType.Main, 1, viewerId: 7L);

        var chara2 = resp.LeaderList.Single(l => l.CharaId == 2);
        Assert.That(chara2.CurrentChapter, Is.EqualTo(6));
        Assert.That(chara2.IsFinished, Is.False);  // chapter 6 not done yet
    }

    [Test]
    public async Task GetSectionsAsync_passes_through_spoiler_fields_from_section_master()
    {
        // Limited-story sections (section_id >= 9000) sit inside main-story worlds and prod uses
        // is_spoiler=1 + spoiler_message="story_section_N" to hide the section name until you've
        // cleared main section N. Verified against prod /story/section responses where section
        // 9003 carries is_spoiler=1, spoiler_message="story_section_14".
        _master.Setup(m => m.GetSectionsByFamilyAsync(StoryApiType.Main))
               .ReturnsAsync(new List<StorySection> {
                   new() { Id = 9003, WorldId = 1, StoryApiType = StoryApiType.Limited,
                           IsLeaderSelect = false, IsSpoiler = 1, SpoilerMessage = "story_section_14" } });
        _master.Setup(m => m.GetWorldsForSectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryWorld> { new() { Id = 1, TitleTextKey = "world_1" } });
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryChapter>());
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var resp = await _service.GetSectionsAsync(StoryApiType.Main, viewerId: 7L);

        var section = resp.WorldList["1"].SectionList.Single(s => s.SectionId == "9003");
        Assert.That(section.IsSpoiler, Is.EqualTo(1));
        Assert.That(section.SpoilerMessage, Is.EqualTo("story_section_14"));
    }

    [Test]
    public async Task GetSectionsAsync_tutorial_section_is_finished_when_viewer_tutorial_state_at_end_step()
    {
        // The tutorial section (id=0) has zero chapter rows — the prologue is hardcoded
        // client-side in Wizard/Prologue.cs. Prod nonetheless returns is_finished=true once
        // viewer.tutorial_step reaches 100; the client uses that to flip IsTutorialReplay,
        // which is what re-enables chapter switching in AreaSelectUI.OnTouchChapterListTutorial.
        // Verified against traffic_prod_626_story.ndjson btn_story_tutorial entry.
        const long viewerId = 7L;
        var service = NewServiceWithSeededViewerTutorialState(
            nameof(GetSectionsAsync_tutorial_section_is_finished_when_viewer_tutorial_state_at_end_step),
            viewerId, tutorialState: 100);

        _master.Setup(m => m.GetSectionsByFamilyAsync(StoryApiType.Main))
               .ReturnsAsync(new List<StorySection> {
                   new() { Id = 0, WorldId = 1, StoryApiType = StoryApiType.Main, IsLeaderSelect = false } });
        _master.Setup(m => m.GetWorldsForSectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryWorld> { new() { Id = 1, TitleTextKey = "world_1" } });
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryChapter>());
        _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var resp = await service.GetSectionsAsync(StoryApiType.Main, viewerId);

        var section = resp.WorldList["1"].SectionList.Single(s => s.SectionId == "0");
        Assert.That(section.IsFinished, Is.True);
    }

    [Test]
    public async Task GetSectionsAsync_tutorial_section_not_finished_when_viewer_mid_tutorial()
    {
        // Mid-tutorial (step < 100): client still needs IsTutorialReplay=false so the AreaSelectUI
        // confirm-only-current-step flow runs. is_finished must stay false.
        const long viewerId = 7L;
        var service = NewServiceWithSeededViewerTutorialState(
            nameof(GetSectionsAsync_tutorial_section_not_finished_when_viewer_mid_tutorial),
            viewerId, tutorialState: 41);

        _master.Setup(m => m.GetSectionsByFamilyAsync(StoryApiType.Main))
               .ReturnsAsync(new List<StorySection> {
                   new() { Id = 0, WorldId = 1, StoryApiType = StoryApiType.Main, IsLeaderSelect = false } });
        _master.Setup(m => m.GetWorldsForSectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryWorld> { new() { Id = 1, TitleTextKey = "world_1" } });
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new List<StoryChapter>());
        _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var resp = await service.GetSectionsAsync(StoryApiType.Main, viewerId);

        var section = resp.WorldList["1"].SectionList.Single(s => s.SectionId == "0");
        Assert.That(section.IsFinished, Is.False);
    }

    private StoryService NewServiceWithSeededViewerTutorialState(string dbName, long viewerId, int tutorialState)
    {
        var db = StoryServiceTestHelpers.NewInMemoryDb(dbName);
        db.Viewers.Add(new SVSim.Database.Models.Viewer {
            Id = viewerId,
            MissionData = new SVSim.Database.Models.ViewerMissionData { TutorialState = tutorialState },
        });
        db.SaveChanges();
        return new StoryService(
            _master.Object, _viewer.Object,
            inv: new Mock<IInventoryService>().Object,
            db: db,
            configService: StoryServiceTestHelpers.NewConfigService(),
            deckRepository: new Mock<SVSim.Database.Repositories.Deck.IDeckRepository>().Object,
            buildDecks: new Mock<SVSim.Database.Repositories.BuildDeck.IBuildDeckRepository>().Object,
            viewers: new Mock<SVSim.Database.Repositories.Viewer.IViewerRepository>().Object,
            xp: new Mock<SVSim.Database.Services.BattleXp.IBattleXpService>().Object,
            logger: NullLogger<StoryService>.Instance);
    }

    [Test]
    public async Task GetLeaderSelectAsync_section_with_custom_leaders_returns_only_those_charas_in_min_story_id_order()
    {
        // Section 17 in prod offers 4 custom leaders (chara_ids 500901-500904), not the default
        // 8 classes. Ordering is by ascending min(story_id) of each chara's chapters:
        // 500901 (569), 500903 (591), 500904 (594), 500902 (597) — non-numeric chara_id sequence.
        // Verified against data_dumps/captures/traffic_prod_626_story.ndjson section_id=17 leader_select.
        var s17chapters = new List<StoryChapter> {
            Ch(569, 17, 500901, "1", "2"),  Ch(570, 17, 500901, "2", "3"),
            Ch(591, 17, 500903, "1", "2"),  Ch(592, 17, 500903, "2", "3"),
            Ch(594, 17, 500904, "1", "2"),  Ch(595, 17, 500904, "2", "3"),
            Ch(597, 17, 500902, "1", "2"),  Ch(598, 17, 500902, "2", "3"),
        };
        _master.Setup(m => m.GetChaptersBySectionsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(17))))
               .ReturnsAsync(s17chapters);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var resp = await _service.GetLeaderSelectAsync(StoryApiType.Main, 17, viewerId: 7L);

        Assert.That(resp.LeaderCount, Is.EqualTo(4));
        Assert.That(resp.LeaderList.Select(l => l.CharaId),
            Is.EqualTo(new[] { 500901, 500903, 500904, 500902 }));
        Assert.That(resp.LeaderList.All(l => l.CurrentChapter == 1), Is.True);
    }

    [Test]
    public async Task StartAsync_returns_sbs_payload_for_chapter_with_sbs_id()
    {
        var chapter = Ch(101, 1, 2, "2", "3", sbsId: 8);
        var sbs = new SpecialBattleSetting { Id = 8, PlayerStartLife = 20, EnemyStartLife = 10,
                                              Note = "Disaster Tree ch2&3" };
        _master.Setup(m => m.GetChapterByIdAsync(101)).ReturnsAsync(chapter);
        _master.Setup(m => m.GetSbsByIdAsync(8)).ReturnsAsync(sbs);

        var resp = await _service.StartAsync(StoryApiType.Main, new[] { 101 }, viewerId: 7L);

        Assert.That(resp.ContainsKey("0"), Is.True);
        var slot = (StartSlotWithSbs)resp["0"];
        Assert.That(slot.SpecialBattleSetting.Id, Is.EqualTo("8"));
        Assert.That(slot.SpecialBattleSetting.EnemyStartLife, Is.EqualTo("10"));
        Assert.That(resp.ContainsKey("mission_parameter"), Is.True);
        Assert.That(((Array)resp["mission_parameter"]).Length, Is.EqualTo(0));
    }

    [Test]
    public async Task StartAsync_returns_empty_slot_for_chapter_without_sbs_id()
    {
        var chapter = Ch(100, 1, 2, "1", "2", sbsId: null);
        _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);

        var resp = await _service.StartAsync(StoryApiType.Main, new[] { 100 }, viewerId: 7L);

        Assert.That(resp["0"], Is.InstanceOf<Array>());
        Assert.That(((Array)resp["0"]).Length, Is.EqualTo(0));
    }

    [Test]
    public async Task GetInfoAsync_emits_sub_chapters_with_per_sub_is_finish()
    {
        // Section 9 ch.13 (story_id 374) carries 5 sub-chapters (374/1, 375/2, 376/3, 377/4, 378/5).
        // The client's SubChapterData parser reads is_finish per sub-chapter to derive the parent's
        // ChapterClearStatus (AllCleared / AlreadyRead / NotCleared). Verified against
        // traffic_prod_more_stories.ndjson section_id=9 /info response.
        var parent = Ch(374, 9, 0, "13", "14", battle: false);
        parent.SubChapters.Add(new StorySubChapter { SubChapterId = 1, SubChapterStoryId = 374 });
        parent.SubChapters.Add(new StorySubChapter { SubChapterId = 2, SubChapterStoryId = 375 });
        parent.SubChapters.Add(new StorySubChapter { SubChapterId = 3, SubChapterStoryId = 376 });
        var ch14 = Ch(379, 9, 0, "14", "0", battle: false);

        _master.Setup(m => m.GetChaptersBySectionCharaAsync(9, 0))
               .ReturnsAsync(new List<StoryChapter> { parent, ch14 });
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                   { 374, new ViewerStoryProgress { StoryId = 374, IsFinish = true } },
                   { 375, new ViewerStoryProgress { StoryId = 375, IsFinish = true } } });
        _viewer.Setup(v => v.GetBranchUnlockedStoryIdsAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new HashSet<int>());

        var resp = await _service.GetInfoAsync(StoryApiType.Main, 9, 0, viewerId: 7L);

        var ch13 = resp.StoryMasterList.Single(c => c.ChapterId == "13");
        Assert.That(ch13.SubChapters, Has.Count.EqualTo(3));
        var subs = ch13.SubChapters.OrderBy(s => s.SubChapterId).ToList();
        Assert.That(subs[0].StoryId, Is.EqualTo(374));   Assert.That(subs[0].IsFinish, Is.True);
        Assert.That(subs[1].StoryId, Is.EqualTo(375));   Assert.That(subs[1].IsFinish, Is.True);
        Assert.That(subs[2].StoryId, Is.EqualTo(376));   Assert.That(subs[2].IsFinish, Is.False);

        // Regular chapter (no subs) should not carry the sub_chapters key on the wire at all —
        // prod omits it entirely. We leave the DTO property null so the global WhenWritingNull
        // policy drops the key during serialization.
        var c14 = resp.StoryMasterList.Single(c => c.ChapterId == "14");
        Assert.That(c14.SubChapters, Is.Null);
    }

    [Test]
    public async Task FinishAsync_sub_chapter_id_marks_progress_via_resolution()
    {
        // Client sends /finish with the sub-chapter's story_id (e.g. 375), not the parent's.
        // Our chapter table has no row for 375 — GetChapterByIdAsync returns null. The service
        // must fall through to StorySubChapter resolution and upsert progress at the sub's id
        // with isFinish=true, isSkipped=true (sub-chapters are always narrative-only).
        // Confirmed against StoryFinishTask.cs line 391 in decompiled client.
        _master.Setup(m => m.GetChapterByIdAsync(375)).ReturnsAsync((StoryChapter?)null);
        _master.Setup(m => m.FindSubChapterByStoryIdAsync(375))
               .ReturnsAsync(new StorySubChapter { SubChapterId = 2, SubChapterStoryId = 375 });

        var req = new FinishRequest { StoryId = 375, IsFinish = 1, ClassId = null };
        await _service.FinishAsync(StoryApiType.Main, req, viewerId: 7L);

        _viewer.Verify(v => v.UpsertProgressAsync(7L, 375, true, true), Times.Once);
    }

    [Test]
    public async Task FinishAsync_no_battle_chapter_marks_both_isFinish_and_isSkipped_true()
    {
        // Limited-story narrative chapters have battle_exists=false. Prod's /info returns
        // is_finish=true AND is_skipped=true for these once /finish is called — the client uses
        // is_finish for the green "Cleared" badge, so leaving is_finish=false (only is_skipped)
        // renders the blue "AlreadyRead" badge instead. Verified against
        // traffic_prod_limited_stories.ndjson story_id=1 after first /finish.
        var chapter = Ch(100, 9001, 0, "1", "2", battle: false);
        _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var req = new FinishRequest {
            StoryId = 100, IsFinish = 1, ClassId = null,   // play-shape absent (no battle to play)
            SelectionChapterId = null,
        };

        await _service.FinishAsync(StoryApiType.Limited, req, viewerId: 7L);

        _viewer.Verify(v => v.UpsertProgressAsync(7L, 100, true, true), Times.Once);
    }

    [Test]
    public async Task FinishAsync_skip_shape_sets_isSkipped_and_grants_nothing()
    {
        var chapter = Ch(100, 1, 2, "1", "2");
        chapter.Rewards.Add(new StoryChapterReward { RewardType = (UserGoodsType)1, RewardNumber = 20 });
        _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var req = new FinishRequest {
            StoryId = 100, IsFinish = 1,
            SelectionChapterId = null,
            IsSelectAnotherEnd = false,
            ClassId = null,           // play-shape absence → skip
        };

        var resp = await _service.FinishAsync(StoryApiType.Main, req, viewerId: 7L);

        Assert.That(resp.Response.RewardList, Is.Empty);
        Assert.That(resp.Response.StoryRewardList, Is.Empty);
        Assert.That(resp.Response.GetClassExperience, Is.EqualTo("0"));
        _viewer.Verify(v => v.UpsertProgressAsync(7L, 100, null, true), Times.Once);
    }

    [Test]
    public async Task FinishAsync_skip_shape_with_selection_unlocks_branch()
    {
        var parent = Ch(200, 17, 500901, "2", "3a 3b 3c");
        var branch3b = Ch(202, 17, 500901, "3b", "4b");
        _master.Setup(m => m.GetChapterByIdAsync(200)).ReturnsAsync(parent);
        _master.Setup(m => m.GetChaptersBySectionCharaAsync(17, 500901))
               .ReturnsAsync(new List<StoryChapter> { parent, branch3b });
        _viewer.Setup(v => v.GetProgressForChaptersAsync(7L, It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

        var req = new FinishRequest {
            StoryId = 200, IsFinish = 1,
            SelectionChapterId = "3b",
            IsSelectAnotherEnd = false,
        };

        await _service.FinishAsync(StoryApiType.Main, req, viewerId: 7L);

        _viewer.Verify(v => v.UpsertBranchUnlockAsync(7L, 202), Times.Once);
    }

    [Test]
    public async Task FinishAsync_play_shape_first_clear_grants_rewards_and_xp()
    {
        using var factory = new SVSimTestFactory();
        var svc = NewServiceWithSeededViewer(factory, out var scope, out var viewerId);
        using (scope)
        {
            var chapter = Ch(100, 1, 2, "1", "2");
            chapter.Rewards.Add(new StoryChapterReward { RewardType = (UserGoodsType)1, RewardDetailId = 0, RewardNumber = 100 });
            _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);
            _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
                   .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

            var req = new FinishRequest {
                StoryId = 100, IsFinish = 1, ClassId = 2,
                EvolveCount = 0, TotalTurn = 5, DeckNo = 1,
                UseBuildDeck = 0, DeckFormat = 1, Mission = new(),
                RecoveryData = null,
            };

            var resp = await svc.FinishAsync(StoryApiType.Main, req, viewerId: viewerId);

            // Viewer started at RedEther=0; grant of 100 → post-state total = 100.
            Assert.That(resp.Response.RewardList, Has.Count.EqualTo(1));
            Assert.That(resp.Response.RewardList[0].RewardNum, Is.EqualTo("100"));
            // Story XP resolves via DI-registered IBattleXpService → real IGameConfigService
            // (the local NewConfigService mock is passed to StoryService but the XP service
            // pulls its own config from DI). BattleXpConfig.ShippedDefaults(): XpPerWin=200,
            // StoryXpPerClear=null → falls back to XpPerWin=200. Curve L1=50, L2=150 → 200 XP
            // crosses both thresholds: L3 with 0.
            Assert.That(resp.Response.GetClassExperience, Is.EqualTo("200"));
            Assert.That(resp.Response.ClassExperience, Is.EqualTo(0));
            Assert.That(resp.Response.ClassLevel, Is.EqualTo("3"));
            _viewer.Verify(v => v.UpsertProgressAsync(viewerId, 100, true, null), Times.Once);

            // Confirm currency + class XP persisted: fetch fresh viewer from a new scope.
            using var verifyScope = factory.Services.CreateScope();
            var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var freshViewer = await db2.Viewers
                .Include(v => v.Classes).ThenInclude(c => c.Class)
                .FirstAsync(v => v.Id == viewerId);
            Assert.That(freshViewer.Currency.RedEther, Is.EqualTo(100UL));
            var cls2 = freshViewer.Classes.Single(c => c.Class.Id == 2);
            Assert.That(cls2.Level, Is.EqualTo(3));
            Assert.That(cls2.Exp, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task FinishAsync_play_shape_replay_does_not_double_grant()
    {
        using var factory = new SVSimTestFactory();
        var svc = NewServiceWithSeededViewer(factory, out var scope, out var viewerId);
        using (scope)
        {
            var chapter = Ch(100, 1, 2, "1", "2");
            chapter.Rewards.Add(new StoryChapterReward { RewardType = (UserGoodsType)1, RewardDetailId = 0, RewardNumber = 100 });
            _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);
            _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
                   .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                       { 100, new ViewerStoryProgress { StoryId = 100, IsFinish = true } } });

            var req = new FinishRequest { StoryId = 100, IsFinish = 1, ClassId = 2 };
            var resp = await svc.FinishAsync(StoryApiType.Main, req, viewerId: viewerId);

            Assert.That(resp.Response.RewardList, Is.Empty);

            // Currency must not have changed from its seed value of 0.
            using var verifyScope = factory.Services.CreateScope();
            var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var freshViewer = await db2.Viewers.FirstAsync(v => v.Id == viewerId);
            Assert.That(freshViewer.Currency.RedEther, Is.EqualTo(0UL));
        }
    }

    [Test]
    public async Task FinishAsync_battle_after_skip_still_grants_rewards()
    {
        using var factory = new SVSimTestFactory();
        var svc = NewServiceWithSeededViewer(factory, out var scope, out var viewerId);
        using (scope)
        {
            var chapter = Ch(100, 1, 2, "1", "2");
            chapter.Rewards.Add(new StoryChapterReward { RewardType = (UserGoodsType)1, RewardDetailId = 0, RewardNumber = 100 });
            _master.Setup(m => m.GetChapterByIdAsync(100)).ReturnsAsync(chapter);
            // Previously skipped but never finished — should be treated as first clear.
            _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
                   .ReturnsAsync(new Dictionary<int, ViewerStoryProgress> {
                       { 100, new ViewerStoryProgress { StoryId = 100, IsFinish = false, IsSkipped = true } } });

            var req = new FinishRequest { StoryId = 100, IsFinish = 1, ClassId = 2 };
            var resp = await svc.FinishAsync(StoryApiType.Main, req, viewerId: viewerId);

            Assert.That(resp.Response.RewardList, Has.Count.EqualTo(1));
            Assert.That(resp.Response.RewardList[0].RewardNum, Is.EqualTo("100"));

            using var verifyScope = factory.Services.CreateScope();
            var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var freshViewer = await db2.Viewers.FirstAsync(v => v.Id == viewerId);
            Assert.That(freshViewer.Currency.RedEther, Is.EqualTo(100UL));
        }
    }

    [Test]
    public async Task FinishAsync_play_shape_first_clear_grants_card_and_cascades_cosmetic()
    {
        using var factory = new SVSimTestFactory();

        const long testCardId = 998_001_010L;
        const int  testSkinId = 998_001_011;
        const int  testStoryId = 998_001_500;

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Cards.Add(new ShadowverseCardEntry { Id = testCardId, Name = "StoryCascadeCard", Rarity = SVSim.Database.Enums.Rarity.Gold });
            db.LeaderSkins.Add(new LeaderSkinEntry { Id = testSkinId, Name = "StoryCascadeSkin" });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = testCardId,
                Type = SVSim.Database.Enums.CosmeticType.Skin,
                CosmeticId = testSkinId,
                Quantity = 1,
            });
            await db.SaveChangesAsync();
        }

        var svc = NewServiceWithSeededViewer(factory, out var scope, out var viewerId);
        using (scope)
        {
            var chapter = Ch(testStoryId, 1, 2, "1", "2");
            chapter.Rewards.Add(new StoryChapterReward
            {
                RewardType = (UserGoodsType)5,                  // UserGoodsType.Card
                RewardDetailId = testCardId,
                RewardNumber = 1,
            });
            _master.Setup(m => m.GetChapterByIdAsync(testStoryId)).ReturnsAsync(chapter);
            _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
                   .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

            var req = new FinishRequest { StoryId = testStoryId, IsFinish = 1, ClassId = 2 };
            var resp = await svc.FinishAsync(StoryApiType.Main, req, viewerId: viewerId);

            // reward_list (post-state) gets BOTH the Card entry AND the cascaded Skin entry.
            Assert.That(resp.Response.RewardList.Any(r => r.RewardType == "5" && r.RewardId == testCardId.ToString()), Is.True,
                "card reward should appear in reward_list");
            Assert.That(resp.Response.RewardList.Any(r => r.RewardType == "10" && r.RewardId == testSkinId.ToString()), Is.True,
                "cascade skin should appear in reward_list");

            // story_reward_list (deltas) only carries the top-level chapter reward.
            Assert.That(resp.Response.StoryRewardList.Count(r => r.RewardType == "5"), Is.EqualTo(1));
            Assert.That(resp.Response.StoryRewardList.Any(r => r.RewardType == "10"), Is.False,
                "cascade cosmetics should not appear in story_reward_list deltas");
        }

        // Verify viewer ownership was persisted.
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await verifyDb.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.LeaderSkins)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.Any(c => c.Card.Id == testCardId), Is.True);
        Assert.That(viewer.LeaderSkins.Any(s => s.Id == testSkinId), Is.True);
    }

    [Test]
    public async Task FinishAsync_card_grant_for_already_owned_card_increments_not_duplicates()
    {
        using var factory = new SVSimTestFactory();

        const long testCardId = 998_002_010L;
        const int  testStoryId = 998_002_500;

        // Pre-seed the card in the catalog AND give the viewer 2 copies of it before the story finish.
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Cards.Add(new ShadowverseCardEntry
            {
                Id = testCardId,
                Name = "ExistingOwnedCard",
                Rarity = SVSim.Database.Enums.Rarity.Silver,
            });
            await db.SaveChangesAsync();
        }

        var svc = NewServiceWithSeededViewer(factory, out var scope, out var viewerId);
        using (scope)
        {
            // Seed 2 owned copies of the card under the same viewer used by NewServiceWithSeededViewer.
            var scopeDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var seedViewer = await scopeDb.Viewers
                .Include(v => v.Cards).ThenInclude(c => c.Card)
                .FirstAsync(v => v.Id == viewerId);
            var card = await scopeDb.Cards.FirstAsync(c => c.Id == testCardId);
            seedViewer.Cards.Add(new OwnedCardEntry { Card = card, Count = 2, IsProtected = false });
            await scopeDb.SaveChangesAsync();

            // Configure a chapter that grants 1 copy of the same card.
            var chapter = Ch(testStoryId, 1, 2, "1", "2");
            chapter.Rewards.Add(new StoryChapterReward
            {
                RewardType = (UserGoodsType)5,                  // UserGoodsType.Card
                RewardDetailId = testCardId,
                RewardNumber = 1,
            });
            _master.Setup(m => m.GetChapterByIdAsync(testStoryId)).ReturnsAsync(chapter);
            _viewer.Setup(v => v.GetProgressForChaptersAsync(viewerId, It.IsAny<IEnumerable<int>>()))
                   .ReturnsAsync(new Dictionary<int, ViewerStoryProgress>());

            var req = new FinishRequest { StoryId = testStoryId, IsFinish = 1, ClassId = 2 };
            var resp = await svc.FinishAsync(StoryApiType.Main, req, viewerId: viewerId);

            // Post-state count on the wire should be 3 (2 owned + 1 granted).
            var cardEntry = resp.Response.RewardList.SingleOrDefault(r => r.RewardType == "5" && r.RewardId == testCardId.ToString());
            Assert.That(cardEntry, Is.Not.Null, "card reward should appear in reward_list");
            Assert.That(cardEntry!.RewardNum, Is.EqualTo("3"), "post-state count should be incremented, not reset to 1");
        }

        // Verify the viewer has exactly ONE OwnedCardEntry row for this card, with Count=3.
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await verifyDb.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId);
        var ownedRows = viewer.Cards.Where(c => c.Card.Id == testCardId).ToList();
        Assert.That(ownedRows, Has.Count.EqualTo(1), "exactly one OwnedCardEntry row should exist (no duplicates)");
        Assert.That(ownedRows[0].Count, Is.EqualTo(3));
    }
}

internal static class StoryServiceTestHelpers
{
    public static SVSim.Database.Services.IGameConfigService NewConfigService()
    {
        var mock = new Mock<SVSim.Database.Services.IGameConfigService>();
        mock.Setup(s => s.Get<SVSim.Database.Models.Config.StoryConfig>())
            .Returns(new SVSim.Database.Models.Config.StoryConfig());
        return mock.Object;
    }

    /// <summary>
    /// Returns a minimal <see cref="SVSimDbContext"/> backed by the EF InMemory provider.
    /// Safe for non-reward tests that never actually query the DB.
    /// The supplied <paramref name="dbName"/> is suffixed with a fresh Guid so concurrent
    /// callers never share a database — EF InMemory keys by name, and the previous callers
    /// all passed the literal string "SetUp" via <c>nameof(SetUp)</c>, which collapsed every
    /// test in the fixture onto the same store and broke under parallel execution.
    /// </summary>
    public static SVSimDbContext NewInMemoryDb(string dbName)
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<SVSimDbContext>()
            .UseInMemoryDatabase($"{dbName}-{Guid.NewGuid():N}")
            .Options;
        return new SVSimDbContext(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SVSimDbContext>.Instance,
            options);
    }
}
