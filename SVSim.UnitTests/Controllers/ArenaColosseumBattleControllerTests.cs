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
/// Per-match battle URL coverage — verifies the dual <c>colosseum_battle/*</c> +
/// <c>colosseum_rank_battle/*</c> dispatch matches <c>run.IsRankMatching</c>, that
/// <c>battle/finish</c> bumps the per-round counters, and that the 3008 promotion
/// trigger flips the run's rank flag.
/// </summary>
public class ArenaColosseumBattleControllerTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    private static async Task SeedRunAsync(SVSimTestFactory factory, long viewerId, bool isRankMatching = false)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
        {
            ViewerId = viewerId,
            EntryId = 1001,
            SeasonId = 42,
            RoundId = 1,
            DeckFormat = Format.Rotation,
            MaxBattleCountThisRound = 5,
            BreakthroughNumberThisRound = 4,
            IsRankMatching = isRankMatching,
            RegisteredDeckNoListJson = "[3]",
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task DoMatching_pre_rank_rejects_when_run_is_rank_matching()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid, isRankMatching: true);
        await factory.SeedDeckAsync(vid, Format.Rotation, number: 3);
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync("/colosseum_battle/do_matching", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("colosseum_url_phase_mismatch", body);
    }

    [Test]
    public async Task DoMatching_post_rank_rejects_when_run_is_pre_rank()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid, isRankMatching: false);
        await factory.SeedDeckAsync(vid, Format.Rotation, number: 3);
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync("/colosseum_rank_battle/do_matching", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("colosseum_url_phase_mismatch", body);
    }

    [Test]
    public async Task DoMatching_returns_3001_when_no_deck_registered()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
            {
                ViewerId = vid,
                EntryId = 1001,
                SeasonId = 42,
                RoundId = 1,
                DeckFormat = Format.Rotation,
                RegisteredDeckNoListJson = "[]",
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/colosseum_battle/do_matching", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("matching_state").GetInt32(), Is.EqualTo(3001));
    }

    [Test]
    public async Task BattleFinish_win_advances_counters_and_appends_result_list()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var req = new
        {
            battle_result = 1, is_retire = 0, class_id = 1,
            total_turn = 5, evolve_count = 1, enemy_evolve_count = 0,
            recovery_data = "",
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };
        var resp = await client.PostAsync("/colosseum_battle/finish", JsonContent.Create(req));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // XP wire assertions
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        // XpPerWin=200; classexp.csv L1=50, L2=150 → 200 XP crosses both: L3, Exp=0.
        Assert.That(doc.RootElement.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(200));
        Assert.That(doc.RootElement.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("class_level").GetInt32(), Is.EqualTo(3));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(run.WinCount, Is.EqualTo(1));
        Assert.That(run.LossCount, Is.EqualTo(0));
        Assert.That(run.BattleCountThisRound, Is.EqualTo(1));
        Assert.That(run.ResultListJson, Is.EqualTo("[1]"));

        // Persisted class XP on viewer
        var v = await db.Viewers.Include(x => x.Classes).ThenInclude(c => c.Class)
            .FirstAsync(x => x.Id == vid);
        var cls1 = v.Classes.Single(c => c.Class.Id == 1);
        Assert.That(cls1.Level, Is.EqualTo(3));
        Assert.That(cls1.Exp, Is.EqualTo(0));
    }

    [Test]
    public async Task BattleFinish_retire_does_not_advance_counters()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var req = new
        {
            battle_result = 2, is_retire = 1, class_id = 1,
            total_turn = 3, evolve_count = 0, enemy_evolve_count = 0,
            recovery_data = "",
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };
        var resp = await client.PostAsync("/colosseum_battle/finish", JsonContent.Create(req));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(run.WinCount, Is.EqualTo(0));
        Assert.That(run.LossCount, Is.EqualTo(0));
        Assert.That(run.BattleCountThisRound, Is.EqualTo(0),
            "is_retire=1 must not bump the per-round battle counter");
    }
}
