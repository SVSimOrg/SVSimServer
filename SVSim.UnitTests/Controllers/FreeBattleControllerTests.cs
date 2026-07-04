using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

[TestFixture]
public class FreeBattleControllerTests
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

    [Test]
    public async Task DoMatching_unlimited_first_poll_returns_3002_RETRY_with_empty_node_server_url()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, 1);
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/unlimited_free_battle/do_matching", DoMatchingBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True, $"Expected 2xx, got {resp.StatusCode}");
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement;
        Assert.That(data.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));
        Assert.That(data.GetProperty("node_server_url").GetString(), Is.EqualTo(""),
            "Empty string, not absent — Phase 2 fix pattern.");
    }

    [Test]
    public async Task DoMatching_unlimited_two_viewers_pair_PvP()
    {
        await using var factory = new SVSimTestFactory();
        var v1 = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_011UL, displayName: "Alice");
        var v2 = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_012UL, displayName: "Bob");
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(v1, Format.Unlimited, 1);
        await factory.SeedDeckAsync(v2, Format.Unlimited, 1);

        // Alice polls first → parks.
        var c1 = factory.CreateAuthenticatedClient(v1);
        var r1 = await c1.PostAsJsonAsync("/unlimited_free_battle/do_matching", DoMatchingBody);
        var j1 = JsonDocument.Parse(await r1.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j1.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));

        // Bob polls — pairs, returns joiner (3004).
        var c2 = factory.CreateAuthenticatedClient(v2);
        var r2 = await c2.PostAsJsonAsync("/unlimited_free_battle/do_matching", DoMatchingBody);
        var j2 = JsonDocument.Parse(await r2.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j2.GetProperty("matching_state").GetInt32(), Is.EqualTo(3004), "Joiner = 3004.");
        Assert.That(j2.GetProperty("battle_id").GetString(), Is.Not.Null.And.Not.Empty);
        Assert.That(j2.GetProperty("node_server_url").GetString(), Is.Not.Empty);

        // Alice polls again — gets cached match, owner role (3007).
        var r3 = await c1.PostAsJsonAsync("/unlimited_free_battle/do_matching", DoMatchingBody);
        var j3 = JsonDocument.Parse(await r3.Content.ReadAsStringAsync()).RootElement;
        Assert.That(j3.GetProperty("matching_state").GetInt32(), Is.EqualTo(3007), "Owner = 3007.");
        Assert.That(j3.GetProperty("battle_id").GetString(), Is.EqualTo(j2.GetProperty("battle_id").GetString()));
    }

    [Test]
    public async Task DoMatching_returns_3001_when_viewer_has_no_deck_for_format_and_slot()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        // Note: NO SeedDeckAsync. BuildForRankBattleAsync will throw InvalidOperationException.
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/unlimited_free_battle/do_matching", DoMatchingBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var data = doc.RootElement;
        Assert.That(data.GetProperty("matching_state").GetInt32(), Is.EqualTo(3001),
            "Missing deck → RC_BATTLE_MATCHING_ILLEGAL.");
        Assert.That(data.GetProperty("node_server_url").GetString(), Is.EqualTo(""));
    }

    [Test]
    public async Task Finish_win_grants_class_xp_with_strict_field_subset()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync(
            "/unlimited_free_battle/finish", FinishBody(battleResult: 1, classId: 1));

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement;
        Assert.That(data.GetProperty("battle_result").GetInt32(), Is.EqualTo(1));
        // XpPerWin=200; classexp.csv L1=50, L2=150 → 200 XP crosses both: L3, Exp=0.
        Assert.That(data.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(200));
        Assert.That(data.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(data.GetProperty("class_level").GetInt32(), Is.EqualTo(3));

        // Strict subset — no rank fields. The client doesn't read them on free-battle
        // finish; emitting them would be wire-format pollution.
        Assert.That(raw, Does.Not.Contain("\"rank\""));
        Assert.That(raw, Does.Not.Contain("\"after_battle_point\""));
        Assert.That(raw, Does.Not.Contain("\"after_master_point\""));
        Assert.That(raw, Does.Not.Contain("\"successive_win"));
    }

    [Test]
    public async Task Finish_loss_grants_loss_xp_on_rotation_url()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync(
            "/rotation_free_battle/finish", FinishBody(battleResult: 2, classId: 1));

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("battle_result").GetInt32(), Is.EqualTo(2));
        // XpPerLoss=50 exactly meets L1 threshold → L2, Exp=0.
        Assert.That(doc.RootElement.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(50));
        Assert.That(doc.RootElement.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("class_level").GetInt32(), Is.EqualTo(2));
    }

    [Test]
    public async Task ForceFinish_returns_200_with_empty_object()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsJsonAsync("/free_battle/force_finish", EmptyAuthedBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True);
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        // Empty object: no enumerable properties.
        Assert.That(doc.RootElement.EnumerateObject().MoveNext(), Is.False,
            "Body should be {} — defensive no-op for dead URL.");
    }

    [Test]
    public async Task DoMatching_unlimited_accepts_literal_prod_request_keys()
    {
        await using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, 7);
        var client = factory.CreateAuthenticatedClient(viewerId);

        // Body shape lifted from a real prod capture (traffic_prod_ranked_unlimited.ndjson
        // line 73). Exercises every wire key the client emits on this URL.
        var literalProdBody = new
        {
            deck_no = 7,
            need_init = 0,
            log = 2,
            excluded_field_id_list = Array.Empty<long>(),
            use_stage_select = 1,
            is_default_skin = 0,
            // Auth placeholders — TestAuthHandler injects the real viewer-id via header.
            viewer_id = "0",
            steam_id = 0,
            steam_session_ticket = "",
        };

        var resp = await client.PostAsJsonAsync("/unlimited_free_battle/do_matching", literalProdBody);

        Assert.That(resp.IsSuccessStatusCode, Is.True, $"Expected 2xx, got {resp.StatusCode}");
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var data = doc.RootElement;
        // First poll on an empty queue → 3002 RETRY.
        Assert.That(data.GetProperty("matching_state").GetInt32(), Is.EqualTo(3002));
        Assert.That(data.GetProperty("node_server_url").GetString(), Is.EqualTo(""));
        // timeout_period + retry_period are always present.
        Assert.That(data.TryGetProperty("timeout_period", out _), Is.True);
        Assert.That(data.TryGetProperty("retry_period", out _), Is.True);
    }
}
