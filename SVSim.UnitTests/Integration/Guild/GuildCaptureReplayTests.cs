using System.Net.Http.Json;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

/// <summary>
/// Capture-replay tests against <c>data_dumps/captures/traffic_prod_guild_create.ndjson</c>.
///
/// The capture bodies are AES-encrypted on the wire, so we can't replay them verbatim.
/// Instead, we issue equivalent plain-JSON requests to the in-process test server and assert
/// that the always-present, session-independent fields in our responses match the prod values
/// extracted from the decrypted capture (the SVSimLoader plugin strips encryption before
/// writing to the capture file, so the JSON bodies are readable).
///
/// Assertions are limited to fields that are globally static (config-driven or shape-driven)
/// and therefore independent of viewer_id, SID, server timestamp, or DB content.
/// </summary>
public class GuildCaptureReplayTests
{
    private const string Vid = "0";
    private const int    Sid = 0;
    private const string Stk = "";

    // Prod values extracted from traffic_prod_guild_create.ndjson lines 19 / 21 / 63 / 55.
    // These are the always-present fields the brief asks us to assert on.
    private static readonly string[] ProdUsableStampList =
        Enumerable.Range(100001, 20).Select(i => i.ToString()).ToArray();

    // ─────────────────────────────────────────────────────────────────────────
    // /guild/info — non-joined viewer
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Replay: first guild/info from the capture (pre-join, non-joined viewer).
    /// Prod response: guild_status="0", max_member_num="30", max_sub_leader_num="2",
    /// usable_stamp_list=["100001".."100020"].
    /// </summary>
    [Test]
    public async Task GuildInfo_NonJoined_MatchesProdConfigFields()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_500_000_001UL,
            displayName: "CaptureReplay_InfoNoGuild");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"guild/info HTTP {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // guild_status="0" (not in a guild)
        Assert.That(root.GetProperty("guild_status").GetString(), Is.EqualTo("0"),
            "Prod guild_status for non-joined viewer must be '0'");

        // max_member_num="30"
        Assert.That(root.GetProperty("max_member_num").GetString(), Is.EqualTo("30"),
            "Prod max_member_num must be '30'");

        // max_sub_leader_num="2"
        Assert.That(root.GetProperty("max_sub_leader_num").GetString(), Is.EqualTo("2"),
            "Prod max_sub_leader_num must be '2'");

        // usable_stamp_list = ["100001".."100020"] exactly
        var stampList = root.GetProperty("usable_stamp_list");
        Assert.That(stampList.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "usable_stamp_list must be an array");
        var actual = stampList.EnumerateArray().Select(e => e.GetString()!).ToArray();
        Assert.That(actual, Is.EqualTo(ProdUsableStampList),
            "usable_stamp_list must match prod exactly (100001..100020)");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /guild/search_guild — empty DB
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Replay: search_guild with empty filter against a fresh DB.
    /// Prod returns a list of guilds; our fresh-DB response returns an empty list.
    /// We assert on shape (list field is an array) rather than content, which will
    /// obviously differ between prod and a test DB with no guilds.
    /// </summary>
    [Test]
    public async Task GuildSearchGuild_EmptyDb_ReturnsListShape()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_500_000_002UL,
            displayName: "CaptureReplay_SearchEmpty");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/guild/search_guild",
            JsonContent.Create(new
            {
                guild_name = "",
                activity = 0,
                join_condition = 0,
                member_condition_range = 0,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk,
            }));

        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"guild/search_guild HTTP {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Shape: "list" must be an array (content will differ — fresh DB = empty)
        Assert.That(root.TryGetProperty("list", out var listEl), Is.True,
            "Response must have a 'list' field");
        Assert.That(listEl.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "'list' must be a JSON array");

        // No result_code=2 error
        if (root.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), "search_guild must not return error result_code");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /guild/emblem_list
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Replay: emblem_list for a viewer who already created a guild.
    /// Prod returns a non-empty list of emblem entries; our test viewer will get
    /// an empty list (no cosmetics in the minimal test DB). We assert on shape only:
    /// guild_emblem_list is an array; no error result_code.
    ///
    /// Note: this endpoint requires the viewer to be in a guild (leader).
    /// We create a guild first so the endpoint resolves correctly.
    /// </summary>
    [Test]
    public async Task GuildEmblemList_Leader_ReturnsEmblemListShape()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_500_000_003UL,
            displayName: "CaptureReplay_EmblemLeader");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Create a guild first (prod capture sequence: /guild/info → /create → /emblem_list)
        var createResp = await client.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = "CaptureReplayGuild",
                activity = 1,
                join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk,
            }));
        Assert.That(createResp.IsSuccessStatusCode, Is.True,
            $"guild/create failed: {await createResp.Content.ReadAsStringAsync()}");

        var resp = await client.PostAsync("/guild/emblem_list",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"guild/emblem_list HTTP {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Shape: guild_emblem_list is an array (prod has 85 entries; ours is empty in test DB)
        Assert.That(root.TryGetProperty("guild_emblem_list", out var listEl), Is.True,
            "Response must have 'guild_emblem_list' field");
        Assert.That(listEl.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "'guild_emblem_list' must be a JSON array");

        // No error
        if (root.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), "emblem_list must not return error");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /guild_chat/messages — member viewer (after guild creation)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Replay: guild_chat/messages after joining/creating a guild.
    /// Prod returns chat_message[], users[], maintenance_card_list[], wait_interval (int).
    /// Our test DB returns empty arrays and a numeric wait_interval from GuildConfig.
    ///
    /// Note: /guild_chat/messages requires the viewer to be a guild member.
    /// We create a guild first to satisfy that precondition.
    /// </summary>
    [Test]
    public async Task GuildChatMessages_Member_ReturnsProdShape()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_500_000_004UL,
            displayName: "CaptureReplay_ChatMember");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Create a guild (becomes leader/member)
        var createResp = await client.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = "ChatReplayGuild",
                activity = 1,
                join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk,
            }));
        Assert.That(createResp.IsSuccessStatusCode, Is.True,
            $"guild/create failed: {await createResp.Content.ReadAsStringAsync()}");

        // Poll messages — start_message_id=0 means "give me everything"
        var resp = await client.PostAsync("/guild_chat/messages",
            JsonContent.Create(new
            {
                start_message_id = 0,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk,
            }));

        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"guild_chat/messages HTTP {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // chat_message must be an array
        Assert.That(root.TryGetProperty("chat_message", out var chatArr), Is.True,
            "Response must have 'chat_message' field");
        Assert.That(chatArr.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "'chat_message' must be a JSON array");

        // wait_interval must be a raw JSON number matching prod (prod sends 3, not "3").
        Assert.That(root.TryGetProperty("wait_interval", out var waitEl), Is.True,
            "Response must have 'wait_interval' field");
        Assert.That(waitEl.ValueKind, Is.EqualTo(JsonValueKind.Number),
            "'wait_interval' must be a raw JSON number (prod sends 3, not \"3\")");
        Assert.That(waitEl.GetInt32(), Is.GreaterThan(0), "'wait_interval' must be positive");

        // maintenance_card_list must be present as array
        Assert.That(root.TryGetProperty("maintenance_card_list", out var maintArr), Is.True,
            "Response must have 'maintenance_card_list' field");
        Assert.That(maintArr.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "'maintenance_card_list' must be a JSON array");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /guild/info — after creating a guild (guild_status="2")
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Replay: guild/info from the capture AFTER the viewer created their guild (line 53).
    /// Prod response has guild_status="2" (JOINING/LEADER) and a populated guild object.
    /// We verify the state transition from "0" (not in guild) to "2" (in guild) matches prod.
    /// </summary>
    [Test]
    public async Task GuildInfo_AfterCreate_GuildStatusIs2()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_500_000_005UL,
            displayName: "CaptureReplay_InfoPostCreate");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Pre-condition: guild_status="0"
        var preinfoResp = await client.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var preinfoJson = await preinfoResp.Content.ReadAsStringAsync();
        using var preDoc = JsonDocument.Parse(preinfoJson);
        Assert.That(preDoc.RootElement.GetProperty("guild_status").GetString(), Is.EqualTo("0"),
            "Pre-create guild_status must be '0'");

        // Create guild
        var createResp = await client.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = "PostCreateGuild",
                activity = 1,
                join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk,
            }));
        Assert.That(createResp.IsSuccessStatusCode, Is.True,
            $"guild/create failed: {await createResp.Content.ReadAsStringAsync()}");

        // Post-condition: guild_status="2" (matches prod line 53 response)
        var postinfoResp = await client.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var postinfoJson = await postinfoResp.Content.ReadAsStringAsync();
        using var postDoc = JsonDocument.Parse(postinfoJson);
        var postRoot = postDoc.RootElement;

        Assert.That(postRoot.GetProperty("guild_status").GetString(), Is.EqualTo("2"),
            "Post-create guild_status must be '2' (JOINING/in-guild) — matches prod line 53");

        // Guild name must be present and populated
        Assert.That(postRoot.TryGetProperty("guild", out var guildEl), Is.True,
            "Post-create response must have a 'guild' object");
        Assert.That(guildEl.ValueKind, Is.EqualTo(JsonValueKind.Object),
            "'guild' must be a JSON object");
    }
}
