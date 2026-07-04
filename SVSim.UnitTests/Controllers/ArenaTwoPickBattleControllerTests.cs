using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.BattleNode.Bridge;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ArenaTwoPickBattleControllerTests
{
    [Test]
    public async Task DoMatching_joiner_Returns3004WithBattleIdAndNodeUrlAndCardMaster()
    {
        using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_021UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_022UL);
        await SeedCompleteTwoPickRunAsync(factory, vidA);
        await SeedCompleteTwoPickRunAsync(factory, vidB);
        using var clientA = factory.CreateAuthenticatedClient(vidA);
        using var clientB = factory.CreateAuthenticatedClient(vidB);

        var req = new {
            deck_no = 1L, need_init = 1, log = 1, excluded_field_id_list = new long[] { }, use_stage_select = 1, is_default_skin = 0,
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };

        // A parks first; B triggers the pair and gets the 3004 joiner response.
        await clientA.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));
        var resp = await clientB.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("matching_state").GetInt32(), Is.EqualTo(3004));
        var battleId = root.GetProperty("battle_id").GetString();
        Assert.That(battleId, Is.Not.Null.And.Not.Empty);
        var nodeUrl = root.GetProperty("node_server_url").GetString();
        Assert.That(nodeUrl, Does.Contain("/socket.io/"));
        Assert.That(nodeUrl, Does.Not.StartWith("ws://"));
        Assert.That(nodeUrl, Does.Not.StartWith("http://"));
        Assert.That(root.GetProperty("card_master_id").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task DoMatching_solo_poller_returns_3002_RETRY_with_no_BattleId_but_empty_NodeServerUrl()
    {
        using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await SeedCompleteTwoPickRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var req = new {
            deck_no = 1L, need_init = 1, log = 1, excluded_field_id_list = new long[] { }, use_stage_select = 1, is_default_skin = 0,
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };
        var resp = await client.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // 3002 = RC_BATTLE_MATCHING_RETRY (client polls again). 3001 is ILLEGAL and
        // pops an error dialog on the client.
        Assert.That(root.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));
        // battle_id must be ABSENT from the JSON; the client's accessor IS guarded with
        // Keys.Contains so absence is the safe shape (matches prod RETRY captures).
        Assert.That(root.TryGetProperty("battle_id", out _), Is.False,
            "battle_id must be absent from the wire when matching_state==3002 RETRY.");
        // node_server_url MUST be present (empty string while waiting, the real URL on
        // SUCCEEDED). Client's DoMatchingBase.SettingDoMatchingData calls .ToString() on
        // it without a Keys.Contains guard, so absence throws KeyNotFoundException.
        Assert.That(root.GetProperty("node_server_url").GetString(), Is.EqualTo(""));
    }

    [Test]
    public async Task DoMatching_two_pollers_get_3004_joiner_and_3007_owner_with_same_BattleId()
    {
        using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_011UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_012UL);
        await SeedCompleteTwoPickRunAsync(factory, vidA);
        await SeedCompleteTwoPickRunAsync(factory, vidB);
        using var clientA = factory.CreateAuthenticatedClient(vidA);
        using var clientB = factory.CreateAuthenticatedClient(vidB);

        var req = new {
            deck_no = 1L, need_init = 1, log = 1, excluded_field_id_list = new long[] { }, use_stage_select = 1, is_default_skin = 0,
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };

        // A polls first (parks).
        var respA1 = await clientA.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));
        using var docA1 = JsonDocument.Parse(await respA1.Content.ReadAsStringAsync());
        Assert.That(docA1.RootElement.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002),
            "A's first poll parks (3002 = RETRY).");

        // B polls and triggers the pair — B is the JOINER (3004).
        var respB = await clientB.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));
        using var docB = JsonDocument.Parse(await respB.Content.ReadAsStringAsync());
        Assert.That(docB.RootElement.GetProperty("matching_state").GetInt32(), Is.EqualTo(3004),
            "B (second arriver, triggered the pair) is the joiner — wire matching_state 3004.");
        var bBattleId = docB.RootElement.GetProperty("battle_id").GetString();
        Assert.That(bBattleId, Is.Not.Null.And.Not.Empty);

        // A polls again, picks up the cached pair — A is the OWNER (3007).
        var respA2 = await clientA.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));
        using var docA2 = JsonDocument.Parse(await respA2.Content.ReadAsStringAsync());
        Assert.That(docA2.RootElement.GetProperty("matching_state").GetInt32(), Is.EqualTo(3007),
            "A (first arriver, picked up cached pair) is the owner — wire matching_state 3007.");
        Assert.That(docA2.RootElement.GetProperty("battle_id").GetString(), Is.EqualTo(bBattleId),
            "Owner and joiner must see the same battle_id.");
        Assert.That(docA2.RootElement.GetProperty("node_server_url").GetString(),
            Is.EqualTo(docB.RootElement.GetProperty("node_server_url").GetString()),
            "Owner and joiner must see the same node_server_url.");
    }


    [Test]
    public async Task DoMatching_NoActiveRun_Returns400WithErrorCode()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = new {
            deck_no = 1L, need_init = 1, log = 1, excluded_field_id_list = new long[] { }, use_stage_select = 1, is_default_skin = 0,
            viewer_id = "0", steam_id = 0, steam_session_ticket = "",
        };
        var resp = await client.PostAsync("/arena_two_pick_battle/do_matching", JsonContent.Create(req));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("error_code").GetString(),
            Is.EqualTo("arena_two_pick_no_active_run"));
    }

    private static async Task SeedCompleteTwoPickRunAsync(SVSimTestFactory factory, long viewerId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var deck = Enumerable.Range(1, 30).Select(i => 100_011_000L + i).ToList();
        db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
        {
            ViewerId = viewerId,
            EntryId = 1,
            ClassId = 1,
            LeaderSkinId = 1,
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
}
