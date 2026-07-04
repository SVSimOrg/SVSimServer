using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.BattleNode.Bridge;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

[TestFixture]
public class MatchContextBuilderTests
{
    [Test]
    public async Task BuildForTwoPick_returns_context_from_run_state()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        var deck = Enumerable.Range(1, 30).Select(i => 100_011_000L + i).ToList();
        int emblemId, degreeId;

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var emblem = await db.Emblems.FirstAsync();
            var degree = await db.Degrees.FirstAsync();
            emblemId = emblem.Id;
            degreeId = degree.Id;

            var viewer = await db.Viewers.FindAsync(vid);
            viewer!.DisplayName = "Drafter";
            viewer.Info.CountryCode = "KOR";
            viewer.Info.IsOfficial = false;
            viewer.Info.SelectedEmblem = emblem;
            viewer.Info.SelectedDegree = degree;
            db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
            {
                ViewerId = vid,
                EntryId = 1,
                ClassId = 5,
                LeaderSkinId = 5_000_001L,
                SelectedCardIdsJson = JsonSerializer.Serialize(deck),
                IsSelectCompleted = true,
                MaxBattleCount = 5,
                CandidateClassIdsJson = "[1,2,3]",
                PendingPickSetsJson = "[]",
                ResultListJson = "[]",
                NextCandidateId = 1,
            });
            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();
        var ctx = await builder.BuildForTwoPickAsync(vid);

        Assert.That(ctx.SelfDeckCardIds, Is.EqualTo(deck));
        Assert.That(ctx.ClassId, Is.EqualTo(CardClass.Shadowcraft));
        Assert.That(ctx.CharaId, Is.EqualTo("5000001"));      // LeaderSkinId set
        Assert.That(ctx.CountryCode, Is.EqualTo("KOR"));
        Assert.That(ctx.UserName, Is.EqualTo("Drafter"));
        Assert.That(ctx.EmblemId, Is.EqualTo(emblemId.ToString()));
        Assert.That(ctx.DegreeId, Is.EqualTo(degreeId.ToString()));
        Assert.That(ctx.IsOfficial, Is.EqualTo(0));
        Assert.That(ctx.BattleModeId, Is.EqualTo(BattleModes.TakeTwo));
        // Hardcoded v1 fixtures (see spec §Deferred plumbing)
        Assert.That(ctx.CardMasterName, Is.EqualTo("card_master_node_10015"));
        Assert.That(ctx.FieldId, Is.EqualTo(43));
        // Sleeve falls back to DefaultLoadoutConfig.SleeveId when the viewer hasn't set
        // a challenge-specific one via /config/update_challenge_config.
        Assert.That(ctx.SleeveId, Is.EqualTo("3000011"));
    }

    [Test]
    public async Task BuildForTwoPick_throws_when_no_run()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => builder.BuildForTwoPickAsync(vid));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_no_active_run"));
    }

    [Test]
    public async Task BuildForTwoPick_throws_when_draft_incomplete()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
            {
                ViewerId = vid,
                EntryId = 1,
                ClassId = 1,
                SelectedCardIdsJson = JsonSerializer.Serialize(new long[] { 100_011_001L, 100_011_002L }),
                IsSelectCompleted = false,
                CandidateClassIdsJson = "[1,2,3]",
                PendingPickSetsJson = "[]",
                ResultListJson = "[]",
                MaxBattleCount = 5,
                NextCandidateId = 1,
            });
            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => builder.BuildForTwoPickAsync(vid));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_draft_incomplete"));
    }

    [Test]
    public async Task BuildForRankBattle_returns_MatchContext_with_format_specific_deck()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "Ranker");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, "Rank Rotation Deck");

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        var ctx = await builder.BuildForRankBattleAsync(viewerId, Format.Rotation, deckNo: 1);

        Assert.That(ctx.UserName, Is.EqualTo("Ranker"));
        Assert.That(ctx.BattleModeId, Is.EqualTo(BattleModes.TakeTwo), "rank-battle carries the same mode id as TK2 on the wire.");
        Assert.That(ctx.ClassId, Is.Not.EqualTo(CardClass.None), "ClassId from the selected deck's class.");
        Assert.That(ctx.CardMasterName, Is.EqualTo("card_master_node_10015"));
        Assert.That(ctx.FieldId, Is.EqualTo(43));
    }

    [Test]
    public async Task BuildForRankBattle_expands_each_deck_card_by_its_count()
    {
        // Regression for the "matched deck only has 1 of each card" battle-node bug:
        // DeckCard is count-based (one row per unique card + a Count), so
        // deck.Cards.Select(c => c.Card.Id) collapsed 3 copies into a single entry.
        // The MatchContext deck must carry one entry PER PHYSICAL CARD.
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "Ranker");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 1, name: "Triples");
        await factory.AddCardToDeckAsync(viewerId, Format.Unlimited, 1, 10001001L, count: 3);
        await factory.AddCardToDeckAsync(viewerId, Format.Unlimited, 1, 10001002L, count: 2);
        await factory.AddCardToDeckAsync(viewerId, Format.Unlimited, 1, 10001003L, count: 1);

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        var ctx = await builder.BuildForRankBattleAsync(viewerId, Format.Unlimited, deckNo: 1);

        Assert.That(ctx.SelfDeckCardIds.Count, Is.EqualTo(6),
            "3 + 2 + 1 copies must produce 6 physical card entries, not 3 unique ids.");
        Assert.That(ctx.SelfDeckCardIds.Count(id => id == 10001001L), Is.EqualTo(3));
        Assert.That(ctx.SelfDeckCardIds.Count(id => id == 10001002L), Is.EqualTo(2));
        Assert.That(ctx.SelfDeckCardIds.Count(id => id == 10001003L), Is.EqualTo(1));
    }

    [Test]
    public async Task BuildForRankBattle_throws_when_no_deck_for_format()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        // Intentionally no SeedDeckAsync for Rotation.

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        Assert.That(async () => await builder.BuildForRankBattleAsync(viewerId, Format.Rotation, deckNo: 1),
            Throws.Exception);
    }

    [Test]
    public async Task BuildForRankBattle_uses_the_caller_supplied_deck_number()
    {
        // Regression for the 2026-06-02 "queued Bloodcraft, saw Swordcraft leader"
        // wire bug — MatchContextBuilder.BuildForRankBattleAsync hardcoded deckNo=1.
        // Seed two decks for different classes (1 and 6) and confirm the deckNo
        // argument picks the right one.
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "Ranker");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 1, name: "Deck 1", classId: 1);
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 5, name: "Deck 5", classId: 6);

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();

        var deck1Ctx = await builder.BuildForRankBattleAsync(viewerId, Format.Unlimited, deckNo: 1);
        var deck5Ctx = await builder.BuildForRankBattleAsync(viewerId, Format.Unlimited, deckNo: 5);

        Assert.That(deck1Ctx.ClassId, Is.EqualTo(CardClass.Forestcraft), "deckNo=1 → class 1.");
        Assert.That(deck5Ctx.ClassId, Is.EqualTo(CardClass.Bloodcraft), "deckNo=5 → class 6 (the wire-bug case).");
    }

    [Test]
    public async Task BuildForTwoPick_falls_back_to_default_loadout_when_unequipped()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        var deck = Enumerable.Range(1, 30).Select(i => 100_011_000L + i).ToList();

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            // No SelectedEmblem / SelectedDegree set → default (Id=0) nav rows.
            db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
            {
                ViewerId = vid,
                EntryId = 1,
                ClassId = 1,
                LeaderSkinId = 0,               // No skin chosen → CharaId == ClassId
                SelectedCardIdsJson = JsonSerializer.Serialize(deck),
                IsSelectCompleted = true,
                CandidateClassIdsJson = "[1,2,3]",
                PendingPickSetsJson = "[]",
                ResultListJson = "[]",
                MaxBattleCount = 5,
                NextCandidateId = 1,
            });
            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();
        var ctx = await builder.BuildForTwoPickAsync(vid);

        // DefaultLoadoutConfig.ShippedDefaults: EmblemId=100000000, DegreeId=300003
        Assert.That(ctx.EmblemId, Is.EqualTo("100000000"));
        Assert.That(ctx.DegreeId, Is.EqualTo("300003"));
        Assert.That(ctx.CharaId, Is.EqualTo("1"));    // falls back to ClassId
    }
}
