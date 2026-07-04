using System.Net.Http.Json;
using System.Text.Json;
using SVSim.Database.Entities.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Guild;

/// <summary>
/// Integration tests for POST /guild_chat/post — NORMAL text and STAMP messages.
/// </summary>
public class GuildChatServicePostTests
{
    private const string Vid = "0";
    private const int    Sid = 0;
    private const string Stk = "";

    private static object BaseBody() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    private static int GetStringifiedInt(JsonElement el, string prop)
        => int.Parse(el.GetProperty(prop).GetString()!);

    private static long GetStringifiedLong(JsonElement el, string prop)
        => long.Parse(el.GetProperty(prop).GetString()!);

    // ─── Helper: create a guild and return its guild_id ──────────────────────

    private static async Task<int> CreateGuildAsync(HttpClient client, string name, int joinCondition = 1)
    {
        var createResp = await client.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = name,
                activity = 1,
                join_condition = joinCondition,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        Assert.That(createResp.IsSuccessStatusCode, Is.True,
            $"create failed: {await createResp.Content.ReadAsStringAsync()}");

        var infoResp = await client.PostAsync("/guild/info",
            JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(infoJson);
        return GetStringifiedInt(doc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");
    }

    // ─── Helper: POST /guild_chat/post ────────────────────────────────────────

    private static async Task<(bool ok, string body)> PostAsync(
        HttpClient client,
        int type,
        string message)
    {
        var resp = await client.PostAsync("/guild_chat/post",
            JsonContent.Create(new
            {
                type,
                message,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var body = await resp.Content.ReadAsStringAsync();
        return (resp.IsSuccessStatusCode, body);
    }

    // ─── Helper: poll messages ────────────────────────────────────────────────

    private static async Task<JsonElement> PollAsync(HttpClient client, int startMessageId = 0, int direction = 1)
    {
        var resp = await client.PostAsync("/guild_chat/messages",
            JsonContent.Create(new
            {
                start_message_id = startMessageId,
                direction        = direction,
                wait_interval    = 3,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"messages HTTP failed: {await resp.Content.ReadAsStringAsync()}");
        var json = await resp.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    // =========================================================================
    // Test 1: Member posts NORMAL — message appears with next message_id
    // =========================================================================

    [Test]
    public async Task Post_normal_message_appears_in_chat()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_001UL, "PostLeader1");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild1");

        var (ok, _) = await PostAsync(leaderClient, type: 0, message: "hello world");
        Assert.That(ok, Is.True, "POST /guild_chat/post should return 200");

        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1 /* OLD */);
        var messages = root.GetProperty("chat_message");

        // At least: CreateGuild + our Normal message
        Assert.That(messages.GetArrayLength(), Is.GreaterThanOrEqualTo(2), "Expected at least CreateGuild + Normal msg");

        bool foundNormal = false;
        for (int i = 0; i < messages.GetArrayLength(); i++)
        {
            int msgType = GetStringifiedInt(messages[i], "message_type");
            if (msgType == (int)GuildChatMessageType.Normal)
            {
                foundNormal = true;
                Assert.That(messages[i].GetProperty("message").GetString(), Is.EqualTo("hello world"),
                    "Normal message body must match");
                Assert.That(GetStringifiedLong(messages[i], "message_id"), Is.GreaterThan(0),
                    "message_id must be positive");
            }
        }
        Assert.That(foundNormal, Is.True, "A Normal message must appear in the chat window");
    }

    // =========================================================================
    // Test 2: Member posts STAMP with valid id — type=1, body is stringified stamp id
    // =========================================================================

    [Test]
    public async Task Post_stamp_with_valid_id_appears_with_type1()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_002UL, "PostLeader2");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild2");

        // ShippedDefaults has stamps 100001..100020; use the first one
        var (ok, _) = await PostAsync(leaderClient, type: 1, message: "100001");
        Assert.That(ok, Is.True, "POST stamp should return 200");

        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1);
        var messages = root.GetProperty("chat_message");

        bool foundStamp = false;
        for (int i = 0; i < messages.GetArrayLength(); i++)
        {
            int msgType = GetStringifiedInt(messages[i], "message_type");
            if (msgType == (int)GuildChatMessageType.Stamp)
            {
                foundStamp = true;
                Assert.That(messages[i].GetProperty("message").GetString(), Is.EqualTo("100001"),
                    "Stamp body must be the stringified stamp id");
            }
        }
        Assert.That(foundStamp, Is.True, "A Stamp message (type=1) must appear in the chat window");
    }

    // =========================================================================
    // Test 3: STAMP id NOT in UsableStampList → result_code=2
    // =========================================================================

    [Test]
    public async Task Post_stamp_invalid_id_returns_error()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_003UL, "PostLeader3");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild3");

        var (ok, body) = await PostAsync(leaderClient, type: 1, message: "999999");
        Assert.That(ok, Is.True, "HTTP should be 200 even for logic errors");

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True, "result_code must be present");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), "result_code must be 2 for invalid stamp");
    }

    // =========================================================================
    // Test 4: Non-member tries to post → result_code=2 (NotInGuild)
    // =========================================================================

    [Test]
    public async Task Post_non_member_returns_error()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_004UL, "PostLeader4");
        long outsiderId = await factory.SeedViewerAsync(76_561_198_600_000_005UL, "PostOutsider4");

        using var leaderClient  = factory.CreateAuthenticatedClient(leaderId);
        using var outsiderClient = factory.CreateAuthenticatedClient(outsiderId);

        await CreateGuildAsync(leaderClient, "PostGuild4");

        var (ok, body) = await PostAsync(outsiderClient, type: 0, message: "hi from outside");
        Assert.That(ok, Is.True, "HTTP 200 even for non-member");

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True, "result_code must be present");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), "result_code must be 2 for non-member");
    }

    // =========================================================================
    // Test 5: Invalid type (e.g., 5 = REPLAY) → result_code=2
    // =========================================================================

    [Test]
    public async Task Post_invalid_type_returns_error()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_006UL, "PostLeader5");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild5");

        var (ok, body) = await PostAsync(leaderClient, type: 5, message: "some replay payload");
        Assert.That(ok, Is.True, "HTTP 200 even for invalid type");

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True, "result_code must be present");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), "result_code must be 2 for type=5");
    }

    // =========================================================================
    // Test 6: Post-then-poll — direction=NEW, start_message_id=<last known> returns posted msg
    // =========================================================================

    [Test]
    public async Task Post_then_poll_NEW_returns_just_posted_message()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_007UL, "PostLeader6");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild6");

        // Get the current max message_id (the CreateGuild system event)
        var beforePoll = await PollAsync(leaderClient, startMessageId: 0, direction: 1);
        var beforeMsgs = beforePoll.GetProperty("chat_message");
        int lastKnownId = 0;
        for (int i = 0; i < beforeMsgs.GetArrayLength(); i++)
        {
            int msgId = (int)GetStringifiedLong(beforeMsgs[i], "message_id");
            if (msgId > lastKnownId) lastKnownId = msgId;
        }

        // Post a Normal message
        var (ok, _) = await PostAsync(leaderClient, type: 0, message: "post-then-poll test");
        Assert.That(ok, Is.True);

        // Poll NEW from lastKnownId — should return exactly our posted message
        var newPoll = await PollAsync(leaderClient, startMessageId: lastKnownId, direction: 2 /* NEW */);
        var newMsgs = newPoll.GetProperty("chat_message");

        Assert.That(newMsgs.GetArrayLength(), Is.GreaterThanOrEqualTo(1),
            "direction=NEW must return at least the just-posted message");

        bool foundPosted = false;
        for (int i = 0; i < newMsgs.GetArrayLength(); i++)
        {
            int msgType = GetStringifiedInt(newMsgs[i], "message_type");
            string? body = newMsgs[i].GetProperty("message").GetString();
            long msgId   = GetStringifiedLong(newMsgs[i], "message_id");

            Assert.That(msgId, Is.GreaterThan(lastKnownId),
                "direction=NEW must return only messages after lastKnownId");
            if (msgType == (int)GuildChatMessageType.Normal && body == "post-then-poll test")
                foundPosted = true;
        }
        Assert.That(foundPosted, Is.True, "The just-posted Normal message must appear in the NEW poll");
    }

    // =========================================================================
    // Test 7: Second post increments message_id
    // =========================================================================

    [Test]
    public async Task Post_twice_second_message_has_higher_message_id()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_600_000_008UL, "PostLeader7");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "PostGuild7");

        await PostAsync(leaderClient, type: 0, message: "first");
        await PostAsync(leaderClient, type: 0, message: "second");

        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1);
        var messages = root.GetProperty("chat_message");

        // Collect message_ids for Normal messages only
        var normalIds = new List<long>();
        for (int i = 0; i < messages.GetArrayLength(); i++)
        {
            if (GetStringifiedInt(messages[i], "message_type") == (int)GuildChatMessageType.Normal)
                normalIds.Add(GetStringifiedLong(messages[i], "message_id"));
        }
        Assert.That(normalIds.Count, Is.EqualTo(2), "Exactly 2 Normal messages expected");
        Assert.That(normalIds[0], Is.LessThan(normalIds[1]),
            "Second message_id must be greater than first");
    }
}
