using System.Net.Http.Json;
using System.Text.Json;
using SVSim.Database.Entities.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

/// <summary>
/// Integration tests for POST /guild_chat/messages — window query + system-event accumulation.
///
/// Uses the REAL GuildChatService (not a spy) so EmitSystemEventAsync actually inserts rows.
/// </summary>
public class GuildChatPollTests
{
    private const string Vid = "0";
    private const int    Sid = 0;
    private const string Stk = "";

    private static object BaseBody() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    private static int GetStringifiedInt(JsonElement el, string prop)
        => int.Parse(el.GetProperty(prop).GetString()!);

    private static long GetStringifiedLong(JsonElement el, string prop)
        => long.Parse(el.GetProperty(prop).GetString()!);

    // ─── Helper: create a guild and return its guild_id ───────────────────────

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

    // ─── Helper: POST /guild_chat/messages and parse response ────────────────

    private static async Task<JsonElement> PollAsync(
        HttpClient client,
        int startMessageId = 0,
        int direction = 1,   // OLD
        int waitInterval = 3)
    {
        var resp = await client.PostAsync("/guild_chat/messages",
            JsonContent.Create(new
            {
                start_message_id = startMessageId,
                direction        = direction,
                wait_interval    = waitInterval,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        Assert.That(resp.IsSuccessStatusCode, Is.True,
            $"messages HTTP failed: {await resp.Content.ReadAsStringAsync()}");
        var json = await resp.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    // =========================================================================
    // Test 1: fresh guild → poll returns exactly one CreateGuild message
    // =========================================================================

    [Test]
    public async Task Poll_freshGuild_direction_OLD_returns_one_CreateGuild_event()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_001UL, "ChatPollLeader1");
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);

        await CreateGuildAsync(leaderClient, "ChatPollGuild1");

        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1 /* OLD */);

        var messages = root.GetProperty("chat_message");
        Assert.That(messages.GetArrayLength(), Is.EqualTo(1),
            $"Expected 1 message after create, got: {root}");

        var msg0 = messages[0];
        Assert.That(GetStringifiedInt(msg0, "message_type"), Is.EqualTo((int)GuildChatMessageType.CreateGuild),
            "First message must be CreateGuild (type=8)");
        Assert.That(GetStringifiedLong(msg0, "viewer_id"), Is.EqualTo(leaderId),
            "Author must be the guild creator");
        Assert.That(GetStringifiedLong(msg0, "message_id"), Is.GreaterThan(0),
            "message_id must be positive");
    }

    // =========================================================================
    // Test 2: leader creates guild → member joins → poll OLD returns both in order
    // =========================================================================

    [Test]
    public async Task Poll_afterJoin_direction_OLD_returns_CreateGuild_then_Join_in_order()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_002UL, "ChatPollLeader2");
        long memberId = await factory.SeedViewerAsync(76_561_198_500_000_003UL, "ChatPollMember2");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var memberClient = factory.CreateAuthenticatedClient(memberId);

        // Create guild (FREE join)
        await CreateGuildAsync(leaderClient, "ChatPollGuild2", joinCondition: 1);

        // Member joins (FREE)
        var joinResp = await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id = 0, // will be looked up — but we need the real one
                from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        // The join needs the real guild_id — let's get it from leader's info first
        var infoResp = await leaderClient.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var infoDoc = JsonDocument.Parse(infoJson);
        int guildId = GetStringifiedInt(infoDoc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");

        // Retry join with real guildId
        var joinResp2 = await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id = guildId,
                from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        Assert.That(joinResp2.IsSuccessStatusCode, Is.True,
            $"join failed: {await joinResp2.Content.ReadAsStringAsync()}");

        // Leader polls
        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1 /* OLD */);

        var messages = root.GetProperty("chat_message");
        Assert.That(messages.GetArrayLength(), Is.EqualTo(2),
            $"Expected 2 messages (CreateGuild + Join), got: {root}");

        // Messages should be ordered oldest-to-newest (ascending message_id)
        long id0 = GetStringifiedLong(messages[0], "message_id");
        long id1 = GetStringifiedLong(messages[1], "message_id");
        Assert.That(id0, Is.LessThan(id1), "Messages must be ordered oldest-to-newest");

        int type0 = GetStringifiedInt(messages[0], "message_type");
        int type1 = GetStringifiedInt(messages[1], "message_type");
        Assert.That(type0, Is.EqualTo((int)GuildChatMessageType.CreateGuild), "First msg must be CreateGuild");
        Assert.That(type1, Is.EqualTo((int)GuildChatMessageType.Join),        "Second msg must be Join");
    }

    // =========================================================================
    // Test 3: direction=NEW returns only messages with message_id > start
    // =========================================================================

    [Test]
    public async Task Poll_direction_NEW_returns_only_messages_after_start()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_004UL, "ChatPollLeader3");
        long memberId = await factory.SeedViewerAsync(76_561_198_500_000_005UL, "ChatPollMember3");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var memberClient = factory.CreateAuthenticatedClient(memberId);

        await CreateGuildAsync(leaderClient, "ChatPollGuild3", joinCondition: 1);

        var infoResp = await leaderClient.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var infoDoc = JsonDocument.Parse(infoJson);
        int guildId = GetStringifiedInt(infoDoc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");

        // Poll to get the CreateGuild message_id
        var firstPoll = await PollAsync(leaderClient, startMessageId: 0, direction: 1 /* OLD */);
        var firstMsgs = firstPoll.GetProperty("chat_message");
        Assert.That(firstMsgs.GetArrayLength(), Is.EqualTo(1), "Should have 1 msg before join");
        int createGuildMsgId = (int)GetStringifiedLong(firstMsgs[0], "message_id");

        // Member joins
        await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id = guildId,
                from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));

        // Poll NEW from createGuildMsgId — should return only the Join message (exclusive: > start)
        var newPoll = await PollAsync(leaderClient, startMessageId: createGuildMsgId, direction: 2 /* NEW */);
        var newMsgs = newPoll.GetProperty("chat_message");

        // direction=NEW uses strict > (exclusive): the CreateGuild message at message_id=createGuildMsgId
        // must NOT appear; only messages with message_id strictly greater than start are returned.
        bool hasCreateGuild = false;
        bool hasJoin = false;
        for (int i = 0; i < newMsgs.GetArrayLength(); i++)
        {
            int msgType = GetStringifiedInt(newMsgs[i], "message_type");
            long msgId  = GetStringifiedLong(newMsgs[i], "message_id");
            if (msgType == (int)GuildChatMessageType.CreateGuild)
                hasCreateGuild = true;
            if (msgType == (int)GuildChatMessageType.Join)
            {
                hasJoin = true;
                Assert.That(msgId, Is.GreaterThan(createGuildMsgId),
                    "Join message_id must be greater than the CreateGuild id");
            }
            // Every returned message must have message_id strictly > start (exclusive semantics)
            Assert.That(msgId, Is.GreaterThan(createGuildMsgId),
                $"direction=NEW must be exclusive: message_id {msgId} must be > start {createGuildMsgId}");
        }
        Assert.That(hasCreateGuild, Is.False,
            "direction=NEW (exclusive) must NOT include the message at start itself");
        Assert.That(hasJoin, Is.True, "direction=NEW poll must include the Join message");
    }

    // =========================================================================
    // Test 4: direction=BOTH returns a window around start_message_id
    // =========================================================================

    [Test]
    public async Task Poll_direction_BOTH_returns_window_around_start()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_006UL, "ChatPollLeader4");
        long memberId = await factory.SeedViewerAsync(76_561_198_500_000_007UL, "ChatPollMember4");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var memberClient = factory.CreateAuthenticatedClient(memberId);

        await CreateGuildAsync(leaderClient, "ChatPollGuild4", joinCondition: 1);

        var infoResp = await leaderClient.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var infoDoc = JsonDocument.Parse(infoJson);
        int guildId = GetStringifiedInt(infoDoc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");

        // Member joins → we now have CreateGuild (id=1) + Join (id=2)
        await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id = guildId,
                from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));

        // BOTH around message_id=1 (the CreateGuild message)
        var bothPoll = await PollAsync(leaderClient, startMessageId: 1, direction: 3 /* BOTH */);
        var bothMsgs = bothPoll.GetProperty("chat_message");

        // With only 2 messages, BOTH around 1 should return at least CreateGuild (id=1) in the result
        Assert.That(bothMsgs.GetArrayLength(), Is.GreaterThanOrEqualTo(1),
            "BOTH direction must return at least 1 message");
        bool hasCreateGuild = false;
        for (int i = 0; i < bothMsgs.GetArrayLength(); i++)
        {
            if (GetStringifiedInt(bothMsgs[i], "message_type") == (int)GuildChatMessageType.CreateGuild)
                hasCreateGuild = true;
        }
        Assert.That(hasCreateGuild, Is.True, "BOTH window around id=1 must include CreateGuild");
    }

    // =========================================================================
    // Test 5: users[] contains deduplicated profiles for all message authors
    // =========================================================================

    [Test]
    public async Task Poll_users_list_contains_all_authors_deduplicated()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_008UL, "ChatPollLeader5");
        long memberId = await factory.SeedViewerAsync(76_561_198_500_000_009UL, "ChatPollMember5");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var memberClient = factory.CreateAuthenticatedClient(memberId);

        await CreateGuildAsync(leaderClient, "ChatPollGuild5", joinCondition: 1);

        var infoResp = await leaderClient.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var infoDoc = JsonDocument.Parse(infoJson);
        int guildId = GetStringifiedInt(infoDoc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");

        // Member joins — now we have 2 messages with 2 different authors
        await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id = guildId,
                from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));

        var root = await PollAsync(leaderClient, startMessageId: 0, direction: 1 /* OLD */);

        var users = root.GetProperty("users");
        Assert.That(users.GetArrayLength(), Is.EqualTo(2),
            $"users[] must have 2 entries (leader + member), got: {root}");

        var userIds = new HashSet<long>();
        for (int i = 0; i < users.GetArrayLength(); i++)
        {
            userIds.Add(GetStringifiedLong(users[i], "viewer_id"));
        }
        Assert.That(userIds, Does.Contain(leaderId),  "users[] must contain the leader's viewer_id");
        Assert.That(userIds, Does.Contain(memberId),  "users[] must contain the member's viewer_id");
    }

    // =========================================================================
    // Test 6: viewer NOT in a guild gets empty result with idle interval
    // =========================================================================

    [Test]
    public async Task Poll_viewerNotInGuild_returns_empty_with_idle_wait_interval()
    {
        using var factory = new SVSimTestFactory();
        long lonelyId = await factory.SeedViewerAsync(76_561_198_500_000_010UL, "ChatPollLonely");
        using var lonelyClient = factory.CreateAuthenticatedClient(lonelyId);

        var root = await PollAsync(lonelyClient, startMessageId: 0, direction: 1);

        var messages = root.GetProperty("chat_message");
        Assert.That(messages.GetArrayLength(), Is.EqualTo(0),
            "chat_message must be empty for a viewer not in a guild");

        // wait_interval is a raw JSON number; idle default is ChatPollIdleSeconds (10)
        Assert.That(root.TryGetProperty("wait_interval", out var waitEl), Is.True,
            "Response must have 'wait_interval' field");
        Assert.That(waitEl.ValueKind, Is.EqualTo(JsonValueKind.Number),
            "'wait_interval' must be a raw JSON number (prod sends 3, not \"3\")");
        Assert.That(waitEl.GetInt32(), Is.GreaterThan(0), "wait_interval must be positive");
    }

    // =========================================================================
    // Test 7: wait_interval is smaller when messages were returned (active)
    //         vs no messages (idle)
    // =========================================================================

    [Test]
    public async Task Poll_activeInterval_less_than_idleInterval_when_messages_returned()
    {
        using var factory = new SVSimTestFactory();
        long leaderId  = await factory.SeedViewerAsync(76_561_198_500_000_011UL, "ChatPollLeader6");
        long lonelyId  = await factory.SeedViewerAsync(76_561_198_500_000_012UL, "ChatPollLonely6");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var lonelyClient = factory.CreateAuthenticatedClient(lonelyId);

        await CreateGuildAsync(leaderClient, "ChatPollGuild6");

        // Leader gets messages → active interval
        var activePoll = await PollAsync(leaderClient, startMessageId: 0, direction: 1);
        int activeWait = activePoll.GetProperty("wait_interval").GetInt32();

        // Lonely viewer not in a guild → idle interval
        var idlePoll = await PollAsync(lonelyClient, startMessageId: 0, direction: 1);
        int idleWait = idlePoll.GetProperty("wait_interval").GetInt32();

        Assert.That(activeWait, Is.LessThan(idleWait),
            $"Active interval ({activeWait}s) must be less than idle interval ({idleWait}s)");
    }
}
