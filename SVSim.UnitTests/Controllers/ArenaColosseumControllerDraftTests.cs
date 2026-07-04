using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Phase 3 2-Pick draft coverage on /arena_colosseum/{get_candidate_classes, class_choose,
/// get_candidate_cards, card_choose}. Mirrors the existing ArenaTwoPick tests, with the
/// pool override sourced from <c>ColosseumSeasonConfig.PoolCardSetIds</c> instead of
/// <c>ChallengeConfig.PoolCardSetIds</c>.
/// </summary>
public class ArenaColosseumControllerDraftTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    private static async Task ActivateChaosCapableSeasonAsync(SVSimTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Pool override: the test card-set's id (10001) — see SVSimTestFactory.SeedMinimalCardSet.
        var seasonJson = JsonSerializer.Serialize(new
        {
            IsColosseumPeriod = true,
            SeasonId = 7,
            DeckFormat = (int)Format.TwoPick,
            IsNormalTwoPick = true,
            PoolCardSetIds = new[] { 10001 },
        });
        await UpsertConfigAsync(db, "ColosseumSeason", seasonJson);
    }

    private static async Task UpsertConfigAsync(SVSimDbContext db, string section, string json)
    {
        var existing = await db.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == section);
        if (existing is null)
            db.GameConfigs.Add(new GameConfigSection { SectionName = section, ValueJson = json });
        else
            existing.ValueJson = json;
        await db.SaveChangesAsync();
    }

    private static async Task SeedRunAsync(SVSimTestFactory factory, long viewerId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
        {
            ViewerId = viewerId,
            EntryId = 9999,
            SeasonId = 7,
            RoundId = 1,
            DeckFormat = Format.TwoPick,
        });

        // The pool service filters cards by `CollectionInfo != null` — the minimal SVSimTestFactory
        // seed lacks that. Stamp it on every card in the test set so the pool service can
        // actually emit a candidate pair for any class.
        var cards = await db.Cards.ToListAsync();
        foreach (var c in cards)
        {
            c.CollectionInfo = new CardCollectionInfo { CraftCost = 40, DustReward = 10 };
        }
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task GetCandidateClasses_seeds_three_classes_on_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateChaosCapableSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync("/arena_colosseum/get_candidate_classes",
            JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("class_id_1").GetInt32(), Is.GreaterThan(0));
        Assert.That(root.GetProperty("class_id_2").GetInt32(), Is.GreaterThan(0));
        Assert.That(root.GetProperty("class_id_3").GetInt32(), Is.GreaterThan(0));

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        var stored = JsonSerializer.Deserialize<List<int>>(run.CandidateClassIdsJson)!;
        Assert.That(stored.Count, Is.EqualTo(3),
            "the slate must be persisted onto the run so /class_choose can validate against it");
    }

    [Test]
    public async Task ClassChoose_rejects_class_not_in_slate()
    {
        using var factory = new SVSimTestFactory();
        await ActivateChaosCapableSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);

        // Force a known slate.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
            run.CandidateClassIdsJson = "[1,2,3]";
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/class_choose",
            JsonContent.Create(new { class_id = 8, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("class_not_offered", body);
    }

    [Test]
    public async Task ClassChoose_normal_advances_run_to_turn_1_with_a_pair_offered()
    {
        using var factory = new SVSimTestFactory();
        await ActivateChaosCapableSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
            run.CandidateClassIdsJson = "[1,2,3]";
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/class_choose",
            JsonContent.Create(new { class_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("class_info").GetProperty("selected_class_id").GetString(), Is.EqualTo("1"),
            "selected_class_id is wire-stringified per existing TwoPick convention");
        Assert.That(root.GetProperty("candidate_card_list").GetArrayLength(), Is.EqualTo(2),
            "the pool service emits exactly 2 candidate pairs per turn");

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var verifyRun = await verifyDb.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(verifyRun.ClassId, Is.EqualTo(1));
        Assert.That(verifyRun.SelectTurn, Is.EqualTo(1));
    }

    [Test]
    public async Task CardChoose_appends_to_selected_cards_and_advances_turn()
    {
        using var factory = new SVSimTestFactory();
        await ActivateChaosCapableSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
            run.CandidateClassIdsJson = "[1,2,3]";
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        // Drive /class_choose to populate the pending pair.
        var classResp = await client.PostAsync("/arena_colosseum/class_choose",
            JsonContent.Create(new { class_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        var classBody = await classResp.Content.ReadAsStringAsync();
        using var classDoc = JsonDocument.Parse(classBody);
        long firstPairId = long.Parse(classDoc.RootElement.GetProperty("candidate_card_list")[0].GetProperty("id").GetString()!);

        var cardResp = await client.PostAsync("/arena_colosseum/card_choose",
            JsonContent.Create(new { selected_id = firstPairId, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(cardResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var verifyRun = await verifyDb.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        var picks = JsonSerializer.Deserialize<List<long>>(verifyRun.SelectedCardIdsJson)!;
        Assert.That(picks.Count, Is.EqualTo(2),
            "first card_choose appends both cards from the picked pair");
        Assert.That(verifyRun.SelectTurn, Is.EqualTo(2));
    }

    [Test]
    public async Task ClassChoose_chaos_branch_stores_chaos_id_on_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateChaosCapableSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/class_choose",
            JsonContent.Create(new { chaos_id = 101, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(run.ChaosId, Is.EqualTo(101));
        Assert.That(run.ClassId, Is.GreaterThan(0),
            "chaos id must resolve to a non-zero class for the pool service");
    }
}
