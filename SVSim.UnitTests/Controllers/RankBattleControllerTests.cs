using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

[TestFixture]
public class RankBattleControllerTests
{
    // BaseRequest fields (viewer_id / steam_id / steam_session_ticket) are required by the
    // request DTOs — the ApiController's auto-validation rejects bodies missing them. We
    // post placeholder values here; the TestAuthHandler injects the real viewer-id via the
    // X-Test-Viewer-Id header set by CreateAuthenticatedClient, so these body values are
    // ignored by auth.
    private static readonly object DoMatchingBody = new
    {
        deck_no = 1,
        need_init = 1,
        log = 0,
        viewer_id = "0",
        steam_id = 0,
        steam_session_ticket = "",
    };

    private static object FinishBody(int battleResult, int classId = 3) => new
    {
        battle_result = battleResult,
        is_retire = 0,
        recovery_data = "{}",
        class_id = classId,
        total_turn = 5,
        viewer_id = "0",
        steam_id = 0,
        steam_session_ticket = "",
    };

    private static readonly object EmptyAuthedBody = new
    {
        viewer_id = "0",
        steam_id = 0,
        steam_session_ticket = "",
    };

    /// <summary>
    /// AiStart in the real client flow always follows a do_matching call that resolved
    /// to 3011 (AI fallback) — that's when the PendingBattle is registered with the
    /// viewer's queue-time MatchContext (deck/cosmetics). Tests bypass do_matching's
    /// time-threshold to register a Bot PendingBattle directly via the bridge.
    /// </summary>
    private static async Task RegisterBotBattleAsync(SVSimTestFactory factory, long viewerId, Format format, int deckNo)
    {
        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IMatchContextBuilder>();
        var ctx = await builder.BuildForRankBattleAsync(viewerId, format, deckNo);
        bridge.RegisterBattle(new BattlePlayer(viewerId, ctx), p2: null, BattleType.Bot);
    }

    [Test]
    public async Task DoMatching_rotation_first_poll_returns_3002_RETRY_with_empty_node_server_url()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/rotation_rank_battle/do_matching", DoMatchingBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True, $"Expected 2xx, got {resp.StatusCode}");
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement;
        Assert.That(data.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));
        Assert.That(data.GetProperty("node_server_url").GetString(), Is.EqualTo(""),
            "Empty string, not absent — Phase 2 fix pattern.");
    }

    [Test]
    public async Task DoMatching_rotation_two_viewers_pair_PvP()
    {
        await using var factory = new SVSimTestFactory();
        var v1 = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL, displayName: "Alice");
        var v2 = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL, displayName: "Bob");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(v1, Format.Rotation, 1);
        await factory.SeedDeckAsync(v2, Format.Rotation, 1);

        // Alice polls first → parks.
        var c1 = factory.CreateAuthenticatedClient(v1);
        var r1 = await c1.PostAsJsonAsync("/rotation_rank_battle/do_matching", DoMatchingBody);
        var j1 = JsonDocument.Parse(await r1.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j1.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));

        // Bob polls — pairs, returns joiner (3004).
        var c2 = factory.CreateAuthenticatedClient(v2);
        var r2 = await c2.PostAsJsonAsync("/rotation_rank_battle/do_matching", DoMatchingBody);
        var j2 = JsonDocument.Parse(await r2.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j2.GetProperty("matching_state").GetInt32(), Is.EqualTo(3004), "Joiner = 3004.");
        Assert.That(j2.GetProperty("battle_id").GetString(), Is.Not.Null.And.Not.Empty);
        Assert.That(j2.GetProperty("node_server_url").GetString(), Is.Not.Empty);

        // Alice polls again — gets cached match, owner role (3007).
        var r3 = await c1.PostAsJsonAsync("/rotation_rank_battle/do_matching", DoMatchingBody);
        var j3 = JsonDocument.Parse(await r3.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j3.GetProperty("matching_state").GetInt32(), Is.EqualTo(3007), "Owner = 3007.");
        Assert.That(j3.GetProperty("battle_id").GetString(), Is.EqualTo(j2.GetProperty("battle_id").GetString()));
    }

    [Test]
    public async Task AiStart_rotation_returns_ai_id_plus_self_oppo_info_camelCase_keys()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "TestViewer");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await RegisterBotBattleAsync(factory, viewerId, Format.Rotation, deckNo: 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/start", EmptyAuthedBody);
        var raw = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement;
        // Series-1 ids from rm_ai_setting.csv — must be one of the real catalog entries.
        Assert.That(data.GetProperty("ai_id").GetInt32(),
            Is.AnyOf(1111, 1121, 1131, 1141, 1151, 1161, 1171, 1181));
        Assert.That(data.GetProperty("turnState").GetInt32(), Is.EqualTo(0));

        // Literal camelCase wire-key checks — these MUST be present verbatim
        // (client uses JsonData.Keys.Contains).
        Assert.That(raw, Does.Contain("\"userName\""), "Wire key must be camelCase, not snake_case.");
        Assert.That(raw, Does.Contain("\"sleeveId\""));
        Assert.That(raw, Does.Contain("\"emblemId\""));
        Assert.That(raw, Does.Contain("\"degreeId\""));
        Assert.That(raw, Does.Contain("\"fieldId\""));
        Assert.That(raw, Does.Contain("\"isOfficial\""));
        Assert.That(raw, Does.Contain("\"classId\""));
        Assert.That(raw, Does.Contain("\"charaId\""));
        Assert.That(raw, Does.Contain("\"isMasterRank\""));
        Assert.That(raw, Does.Contain("\"battlePoint\""));
        Assert.That(raw, Does.Contain("\"masterPoint\""));
        // self_info / oppo_info / country_code stay snake_case (the outliers per ai-start.md).
        Assert.That(raw, Does.Contain("\"self_info\""));
        Assert.That(raw, Does.Contain("\"oppo_info\""));
        Assert.That(raw, Does.Contain("\"country_code\""));
    }

    [Test]
    public async Task AiStart_self_info_reflects_caller_user_name()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "Alice");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await RegisterBotBattleAsync(factory, viewerId, Format.Rotation, deckNo: 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/start", EmptyAuthedBody);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var selfInfo = doc.RootElement.GetProperty("self_info");

        Assert.That(selfInfo.GetProperty("userName").GetString(), Is.EqualTo("Alice"));
    }

    [Test]
    public async Task AiStart_oppo_info_reflects_roster_pick()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "PlayerA");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await RegisterBotBattleAsync(factory, viewerId, Format.Rotation, deckNo: 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/start", EmptyAuthedBody);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var oppoInfo = doc.RootElement.GetProperty("oppo_info");

        // BotRoster's stub names contain "AI" — verify the roster was consulted.
        Assert.That(oppoInfo.GetProperty("userName").GetString(), Does.Contain("AI"));
        Assert.That(oppoInfo.GetProperty("classId").GetInt32(), Is.InRange(1, 8));
    }

    [Test]
    public async Task AiStart_self_info_class_matches_queued_deck_number()
    {
        // Regression for the 2026-06-02 "queued Bloodcraft, saw Swordcraft leader" wire bug.
        // The original impl rebuilt MatchContext from deck #1 inside AiStart; the fix routes
        // it through the PendingBattle the bridge stored at do_matching time (which carries
        // the queue-time deck_no). Seed two distinct-class decks and confirm /ai_*/start
        // returns the right class for a viewer registered with deck #5.
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(displayName: "Ranker");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 1, classId: 1);
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 5, classId: 6);
        await RegisterBotBattleAsync(factory, viewerId, Format.Unlimited, deckNo: 5);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/ai_unlimited_rank_battle/start", EmptyAuthedBody);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var selfInfo = doc.RootElement.GetProperty("self_info");

        Assert.That(selfInfo.GetProperty("classId").GetInt32(), Is.EqualTo(6),
            "Self class must reflect the deck the viewer queued with (deck #5 = Bloodcraft, class 6).");
    }

    [Test]
    public async Task AiStart_without_pending_battle_returns_neg1_sentinel()
    {
        // Defensive: clients always do_matching before ai_start, but if /ai_*/start is hit
        // without a registered PendingBattle (server restart, expired match, ...), the spec
        // sentinel ai_id=-1 surfaces the "no AI assigned" error in the client.
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/start", EmptyAuthedBody);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());

        Assert.That(doc.RootElement.GetProperty("ai_id").GetInt32(), Is.EqualTo(-1));
    }

    [Test]
    public async Task Finish_grants_win_xp_via_ai_variant()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync(
            "/ai_rotation_rank_battle/finish", FinishBody(battleResult: 1, classId: 1));

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var data = doc.RootElement;
        Assert.That(data.GetProperty("battle_result").GetInt32(), Is.EqualTo(1));
        // First-ever win: +100 → Beginner 1 (rank_id=2), Point=100.
        Assert.That(data.GetProperty("rank").GetInt32(), Is.EqualTo(2));
        Assert.That(data.GetProperty("after_battle_point").GetInt32(), Is.EqualTo(100));
        Assert.That(data.GetProperty("battle_point").GetInt32(), Is.EqualTo(100));
        Assert.That(data.GetProperty("after_master_point").GetInt32(), Is.EqualTo(0));
        Assert.That(data.GetProperty("master_point").GetInt32(), Is.EqualTo(0));
        // BattleXpConfig.XpPerWin=200; classexp.csv L1=50, L2=150 → 200 XP crosses both
        // thresholds: land at L3 with 0.
        Assert.That(data.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(200));
        Assert.That(data.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(data.GetProperty("class_level").GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task Finish_with_consistency_result_echoes_2()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/rotation_rank_battle/finish", FinishBody(battleResult: 2, classId: 1));

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("battle_result").GetInt32(), Is.EqualTo(2));
        // battle_result==2 is loss-shaped for rank too: fresh viewer at 0 pts stays at
        // Beginner 0 (tier floor 0). No point change; +/-0 emitted.
        Assert.That(doc.RootElement.GetProperty("rank").GetInt32(), Is.EqualTo(1));
        Assert.That(doc.RootElement.GetProperty("after_battle_point").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("battle_point").GetInt32(), Is.EqualTo(0));
        // battle_result==2 is loss-shaped: XpPerLoss=50 exactly meets L1 threshold → L2, Exp=0.
        Assert.That(doc.RootElement.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(50));
        Assert.That(doc.RootElement.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("class_level").GetInt32(), Is.EqualTo(2));
    }

    [Test]
    public async Task Finish_persists_class_xp_to_viewer()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        await client.PostAsJsonAsync(
            "/unlimited_rank_battle/finish", FinishBody(battleResult: 1, classId: 3));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.Classes).ThenInclude(c => c.Class)
            .Include(x => x.RankProgress)
            .FirstAsync(x => x.Id == viewerId);
        var cls3 = v.Classes.Single(c => c.Class.Id == 3);
        Assert.That(cls3.Level, Is.EqualTo(3));
        Assert.That(cls3.Exp, Is.EqualTo(0));
        // Rank progress persisted under Unlimited with +100.
        var rp = v.RankProgress.Single(r => r.Format == Format.Unlimited);
        Assert.That(rp.Point, Is.EqualTo(100));
        Assert.That(rp.MasterPoint, Is.EqualTo(0));
    }

    [Test]
    public async Task Finish_loss_at_D_tier_stays_at_D_floor()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();

        // Preload viewer with Point=1200 (D0 entry) in Rotation.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.RankProgress).FirstAsync(x => x.Id == viewerId);
            v.RankProgress.Add(new ViewerRankProgress
            {
                Format = Format.Rotation, Point = 1200, MasterPoint = 0,
            });
            await db.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsJsonAsync(
            "/rotation_rank_battle/finish", FinishBody(battleResult: 0, classId: 1));

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var data = doc.RootElement;
        Assert.That(data.GetProperty("rank").GetInt32(), Is.EqualTo(5));                  // still D0
        Assert.That(data.GetProperty("after_battle_point").GetInt32(), Is.EqualTo(1200)); // floored
        Assert.That(data.GetProperty("battle_point").GetInt32(), Is.EqualTo(0));          // no drop
    }

    [Test]
    public async Task Finish_rotation_and_unlimited_progress_independently()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        await client.PostAsJsonAsync("/rotation_rank_battle/finish", FinishBody(battleResult: 1, classId: 1));
        await client.PostAsJsonAsync("/rotation_rank_battle/finish", FinishBody(battleResult: 1, classId: 1));
        await client.PostAsJsonAsync("/unlimited_rank_battle/finish", FinishBody(battleResult: 1, classId: 1));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.RankProgress).FirstAsync(x => x.Id == viewerId);
        Assert.That(v.RankProgress.Single(r => r.Format == Format.Rotation).Point,  Is.EqualTo(200));
        Assert.That(v.RankProgress.Single(r => r.Format == Format.Unlimited).Point, Is.EqualTo(100));
    }

    [Test]
    public async Task Finish_ai_variant_persists_progress_same_as_human_variant()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        await client.PostAsJsonAsync("/ai_unlimited_rank_battle/finish", FinishBody(battleResult: 1, classId: 1));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.RankProgress).FirstAsync(x => x.Id == viewerId);
        Assert.That(v.RankProgress.Single(r => r.Format == Format.Unlimited).Point, Is.EqualTo(100));
    }

    [Test]
    public async Task AiStart_SelfInfo_carries_viewer_rank_progress()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, 1);

        // Preload Point=200 (Beginner 2 = rank_id 3) on Unlimited.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.RankProgress).FirstAsync(x => x.Id == viewerId);
            v.RankProgress.Add(new ViewerRankProgress
            {
                Format = Format.Unlimited, Point = 200, MasterPoint = 0,
            });
            await db.SaveChangesAsync();
        }

        await RegisterBotBattleAsync(factory, viewerId, Format.Unlimited, deckNo: 1);
        var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsJsonAsync("/ai_unlimited_rank_battle/start", EmptyAuthedBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        // AiBattlePlayerInfo wire keys are camelCase (self_info/oppo_info aside);
        // see AiBattleStartResponseDto.cs class comment.
        var self = doc.RootElement.GetProperty("self_info");
        Assert.That(self.GetProperty("rank").GetInt32(), Is.EqualTo(3));            // 200 → Beginner 2
        Assert.That(self.GetProperty("battlePoint").GetInt32(), Is.EqualTo(200));
        Assert.That(self.GetProperty("isMasterRank").GetInt32(), Is.EqualTo(0));
        Assert.That(self.GetProperty("masterPoint").GetInt32(), Is.EqualTo(0));
    }
}
