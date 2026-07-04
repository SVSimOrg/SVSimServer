using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Models.Dtos.Replay;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ReplayControllerTests
{
    // Minimal BaseRequest-shaped body. The translation middleware (in prod) and the
    // [FromBody] BaseRequest _ binding (in tests) both require the auth fields to
    // be present even when their values are unused — same pattern as
    // RankBattleControllerTests.FinishBody. The actual viewer_id comes from the
    // session claim, not the body.
    private static object EmptyBody() => new
    {
        viewer_id = "0",
        steam_id = 0,
        steam_session_ticket = "",
    };

    [Test]
    public async Task ReplayInfo_returns_empty_list_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/replay/info", EmptyBody());
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadFromJsonAsync<ReplayInfoResponseDto>();
        Assert.That(body!.ReplayList, Is.Empty);
    }

    [Test]
    public async Task ReplayInfo_returns_recent_rows_newest_first()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattleHistories.AddRange(
                NewRow(viewerId, battleId: 1, createTime: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                NewRow(viewerId, battleId: 2, createTime: new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc)),
                NewRow(viewerId, battleId: 3, createTime: new DateTime(2026, 6, 3, 0, 0, 0, DateTimeKind.Utc)));
            await db.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient(viewerId);
        var body = await client.PostAsJsonAsync("/replay/info", EmptyBody())
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ReplayInfoResponseDto>())
            .Unwrap();

        Assert.That(body!.ReplayList.Select(r => r.BattleId).ToList(),
            Is.EqualTo(new[] { "3", "2", "1" }));
    }

    [Test]
    public async Task ReplayInfo_does_not_leak_other_viewers_rows()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattleHistories.Add(NewRow(viewerA, battleId: 100, createTime: DateTime.UtcNow));
            db.ViewerBattleHistories.Add(NewRow(viewerB, battleId: 200, createTime: DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient(viewerA);
        var body = await client.PostAsJsonAsync("/replay/info", EmptyBody())
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ReplayInfoResponseDto>())
            .Unwrap();

        Assert.That(body!.ReplayList, Has.Count.EqualTo(1));
        Assert.That(body.ReplayList[0].BattleId, Is.EqualTo("100"));
    }

    [Test]
    public async Task ReplayDetail_returns_non_success_status()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/replay/detail", new
        {
            viewer_id = viewerId,
            battle_id = 234_471_983_876L,
        });

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AiRankFinish_writes_history_row_visible_from_ReplayInfo()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        // Inject a pre-stashed BattleContext as if /ai_rotation_rank_battle/start had run.
        var store = factory.Services.GetRequiredService<IBattleContextStore>();
        store.Set(viewerId, new BattleContext(
            BattleId: 234_471_983_876L,
            BattleType: 2, DeckFormat: 0, TwoPickType: 0,
            SelfClassId: 8, SelfSubClassId: 0, SelfCharaId: 8, SelfRotationId: "0",
            OpponentViewerId: 0, OpponentName: "BotName", OpponentClassId: 5,
            OpponentSubClassId: 0, OpponentCharaId: 805, OpponentCountryCode: "",
            OpponentEmblemId: 721_341_010L, OpponentDegreeId: 120_023L,
            OpponentRotationId: "0",
            BattleStartTime: new DateTime(2026, 6, 4, 17, 13, 13, DateTimeKind.Utc)));

        var client = factory.CreateAuthenticatedClient(viewerId);

        var finishResp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/finish", new
        {
            viewer_id = "0",
            steam_id = 0,
            steam_session_ticket = "",
            battle_result = 1, // win
            class_id = 8,
            total_turn = 7,
            evolve_count = 1,
            enemy_evolve_count = 0,
            sdtrb = 0,
        });
        Assert.That(finishResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var infoBody = await client.PostAsJsonAsync("/replay/info", EmptyBody())
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ReplayInfoResponseDto>())
            .Unwrap();

        Assert.That(infoBody!.ReplayList, Has.Count.EqualTo(1));
        var row = infoBody.ReplayList[0];
        Assert.That(row.BattleId, Is.EqualTo("234471983876"));
        Assert.That(row.OpponentName, Is.EqualTo("BotName"));
        Assert.That(row.IsWin, Is.EqualTo("1"));
        Assert.That(row.OpponentEmblemId, Is.EqualTo("721341010"));
    }

    [Test]
    public async Task AiRankFinish_with_no_stashed_context_does_not_crash()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        var client = factory.CreateAuthenticatedClient(viewerId);

        var finishResp = await client.PostAsJsonAsync("/ai_rotation_rank_battle/finish", new
        {
            viewer_id = "0",
            steam_id = 0,
            steam_session_ticket = "",
            battle_result = 0,
            class_id = 1, total_turn = 1, evolve_count = 0, enemy_evolve_count = 0, sdtrb = 0,
        });
        Assert.That(finishResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var infoBody = await client.PostAsJsonAsync("/replay/info", EmptyBody())
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ReplayInfoResponseDto>())
            .Unwrap();
        Assert.That(infoBody!.ReplayList, Is.Empty);
    }

    [Test]
    public async Task ArenaTwoPickFinish_writes_history_row_visible_from_ReplayInfo()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        // Seed an active arena run so RecordBattleResultAsync doesn't throw no_active_run.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Set<ViewerArenaTwoPickRun>().Add(new ViewerArenaTwoPickRun
            {
                ViewerId = viewerId,
                EntryId = 1,
                ClassId = 1,
                MaxBattleCount = 5,
                WinCount = 0,
                LossCount = 0,
                ResultListJson = "[]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var store = factory.Services.GetRequiredService<IBattleContextStore>();
        store.Set(viewerId, new BattleContext(
            BattleId: 999_888_777L,
            BattleType: 4, DeckFormat: 10, TwoPickType: 0,
            SelfClassId: 1, SelfSubClassId: 0, SelfCharaId: 1, SelfRotationId: "0",
            OpponentViewerId: 0, OpponentName: "TwoPickBot", OpponentClassId: 2,
            OpponentSubClassId: 0, OpponentCharaId: 1, OpponentCountryCode: "",
            OpponentEmblemId: 0, OpponentDegreeId: 0, OpponentRotationId: "0",
            BattleStartTime: DateTime.UtcNow));

        var client = factory.CreateAuthenticatedClient(viewerId);

        var finishResp = await client.PostAsJsonAsync("/arena_two_pick_battle/finish", new
        {
            viewer_id = "0",
            steam_id = 0,
            steam_session_ticket = "",
            battle_result = 1,
        });
        Assert.That(finishResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var infoBody = await client.PostAsJsonAsync("/replay/info", EmptyBody())
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ReplayInfoResponseDto>())
            .Unwrap();
        Assert.That(infoBody!.ReplayList, Has.Count.EqualTo(1));
        Assert.That(infoBody.ReplayList[0].OpponentName, Is.EqualTo("TwoPickBot"));
    }

    private static ViewerBattleHistory NewRow(long viewerId, long battleId, DateTime createTime) => new()
    {
        ViewerId = viewerId,
        BattleId = battleId,
        SelfRotationId = "0",
        OpponentName = "",
        OpponentCountryCode = "",
        OpponentRotationId = "0",
        BattleStartTime = createTime.AddMinutes(-3),
        CreateTime = createTime,
    };
}
