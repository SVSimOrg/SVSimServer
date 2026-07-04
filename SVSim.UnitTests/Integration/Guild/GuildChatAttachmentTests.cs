using System.Net.Http.Json;
using System.Text.Json;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

/// <summary>
/// Integration tests for the five guild_chat attachment endpoints:
///   POST /guild_chat/add_deck, /delete_deck, /add_replay, /replay_detail, /deck_log
/// </summary>
public class GuildChatAttachmentTests
{
    private const string Vid = "0";
    private const int    Sid = 0;
    private const string Stk = "";

    private static object BaseBody() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    private static int GetStringifiedInt(JsonElement el, string prop)
        => int.Parse(el.GetProperty(prop).GetString()!);

    private static long GetStringifiedLong(JsonElement el, string prop)
        => long.Parse(el.GetProperty(prop).GetString()!);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<int> CreateGuildAsync(HttpClient client, string name)
    {
        var createResp = await client.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = name, activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        Assert.That(createResp.IsSuccessStatusCode, Is.True,
            $"create guild failed: {await createResp.Content.ReadAsStringAsync()}");

        var infoResp = await client.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(infoJson);
        return GetStringifiedInt(doc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");
    }

    /// <summary>
    /// Adds a member to the guild using FREE join (join_condition=1).
    /// The guild must have been created with join_condition=1 for this to succeed directly.
    /// </summary>
    private static async Task AddMemberToGuildAsync(
        SVSimTestFactory factory, long leaderId, long memberId, int guildId, string guildName)
    {
        // Get the actual guild_id by querying the leader's guild info.
        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        var infoResp = await leaderClient.PostAsync("/guild/info", JsonContent.Create(BaseBody()));
        var infoBody = await infoResp.Content.ReadAsStringAsync();
        using var infoDoc = JsonDocument.Parse(infoBody);
        int actualGuildId = GetStringifiedInt(infoDoc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");

        // Member joins directly (FREE guild, join_condition=1).
        using var memberClient = factory.CreateAuthenticatedClient(memberId);
        var joinResp = await memberClient.PostAsync("/guild/join",
            JsonContent.Create(new
            {
                guild_id    = actualGuildId,
                from_invite = false,
                viewer_id = memberId.ToString(), steam_id = Sid, steam_session_ticket = Stk
            }));
        var joinBody = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, $"guild/join failed: {joinBody}");
    }

    private static async Task<(bool ok, string body)> AddDeckAsync(HttpClient client, int deckFormat, int deckNo)
    {
        var resp = await client.PostAsync("/guild_chat/add_deck",
            JsonContent.Create(new
            {
                deck_format = deckFormat,
                deck_no     = deckNo,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        return (resp.IsSuccessStatusCode, await resp.Content.ReadAsStringAsync());
    }

    private static async Task<(bool ok, string body)> DeleteDeckAsync(HttpClient client, int deckFormat, long messageId)
    {
        var resp = await client.PostAsync("/guild_chat/delete_deck",
            JsonContent.Create(new
            {
                deck_format = deckFormat,
                message_id  = messageId.ToString(),
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        return (resp.IsSuccessStatusCode, await resp.Content.ReadAsStringAsync());
    }

    private static async Task<(bool ok, string body)> AddReplayAsync(HttpClient client, long battleId)
    {
        var resp = await client.PostAsync("/guild_chat/add_replay",
            JsonContent.Create(new
            {
                battle_id = battleId,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        return (resp.IsSuccessStatusCode, await resp.Content.ReadAsStringAsync());
    }

    private static async Task<(bool ok, JsonElement root)> DeckLogAsync(HttpClient client)
    {
        var resp = await client.PostAsync("/guild_chat/deck_log", JsonContent.Create(BaseBody()));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return (resp.IsSuccessStatusCode, doc.RootElement.Clone());
    }

    private static async Task<(bool ok, JsonElement root)> ReplayDetailAsync(HttpClient client, long messageId)
    {
        var resp = await client.PostAsync("/guild_chat/replay_detail",
            JsonContent.Create(new
            {
                message_id = messageId.ToString(),
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return (resp.IsSuccessStatusCode, doc.RootElement.Clone());
    }

    private static async Task<long> GetLastMessageIdAsync(HttpClient client)
    {
        var resp = await client.PostAsync("/guild_chat/messages",
            JsonContent.Create(new
            {
                start_message_id = 0, direction = 1, wait_interval = 3,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var msgs = doc.RootElement.GetProperty("chat_message");
        long max = 0;
        for (int i = 0; i < msgs.GetArrayLength(); i++)
            max = Math.Max(max, GetStringifiedLong(msgs[i], "message_id"));
        return max;
    }

    // =========================================================================
    // Test 1: add_deck — creates DECK row with DeckPayload; appears in deck_log
    // =========================================================================

    [Test]
    public async Task AddDeck_creates_Deck_message_and_appears_in_deck_log()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_700_000_001UL, "AttLeader1");
        await factory.SeedDeckAsync(leaderId, Format.Rotation, number: 1, name: "MyDeck");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        await CreateGuildAsync(leaderClient, "AttGuild1");

        var (ok, body) = await AddDeckAsync(leaderClient, deckFormat: 1, deckNo: 1);
        Assert.That(ok, Is.True, $"add_deck should return 200; got: {body}");

        // deck_log should now include an entry in bucket "1" (Rotation)
        var (logOk, logRoot) = await DeckLogAsync(leaderClient);
        Assert.That(logOk, Is.True, "deck_log request should succeed");
        Assert.That(logRoot.TryGetProperty("deck_log", out var deckLogEl), Is.True, "deck_log must be present");
        Assert.That(deckLogEl.TryGetProperty("1", out var rotBucket), Is.True, "Rotation bucket '1' must exist");
        Assert.That(rotBucket.GetArrayLength(), Is.EqualTo(1), "One entry in Rotation bucket");

        var entry = rotBucket[0];
        Assert.That(entry.GetProperty("deck_name").GetString(), Is.EqualTo("MyDeck"), "deck_name must match");
        Assert.That(entry.TryGetProperty("message_id", out _), Is.True, "entry must have message_id");
        Assert.That(entry.TryGetProperty("delete_permission_exists", out var dpEl), Is.True);
        Assert.That(dpEl.GetBoolean(), Is.True, "Author can delete own deck");
    }

    // =========================================================================
    // Test 2: add_deck — non-member returns error
    // =========================================================================

    [Test]
    public async Task AddDeck_non_member_returns_error()
    {
        using var factory = new SVSimTestFactory();
        long leaderId    = await factory.SeedViewerAsync(76_561_198_700_000_002UL, "AttLeader2");
        long outsiderId  = await factory.SeedViewerAsync(76_561_198_700_000_003UL, "AttOutsider2");

        using var leaderClient   = factory.CreateAuthenticatedClient(leaderId);
        using var outsiderClient = factory.CreateAuthenticatedClient(outsiderId);

        await factory.SeedDeckAsync(outsiderId, Format.Rotation, 1);
        await CreateGuildAsync(leaderClient, "AttGuild2");

        var (_, body) = await AddDeckAsync(outsiderClient, deckFormat: 1, deckNo: 1);
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True);
        Assert.That(rc.GetInt32(), Is.EqualTo(2), "Non-member must get result_code=2");
    }

    // =========================================================================
    // Test 3: delete_deck (author) — clears payload, row stays, deck_log shrinks
    // =========================================================================

    [Test]
    public async Task DeleteDeck_by_author_clears_payload_and_refreshes_log()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_700_000_004UL, "AttLeader3");
        await factory.SeedDeckAsync(leaderId, Format.Rotation, 1, "DeleteMe");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        await CreateGuildAsync(leaderClient, "AttGuild3");

        var (addOk, _) = await AddDeckAsync(leaderClient, deckFormat: 1, deckNo: 1);
        Assert.That(addOk, Is.True);

        // Get message_id of the DECK row
        long messageId = await GetLastMessageIdAsync(leaderClient);

        // Delete
        var (delOk, delBody) = await DeleteDeckAsync(leaderClient, deckFormat: 1, messageId);
        Assert.That(delOk, Is.True, $"delete_deck should succeed; got: {delBody}");
        using var delDoc = JsonDocument.Parse(delBody);
        // Response must have deck_log + maintenance_card_list
        Assert.That(delDoc.RootElement.TryGetProperty("deck_log", out var afterLog), Is.True,
            "delete_deck must return refreshed deck_log");
        // The Rotation bucket should now be empty
        Assert.That(afterLog.TryGetProperty("1", out var rotBucket), Is.True);
        Assert.That(rotBucket.GetArrayLength(), Is.EqualTo(0), "Deck should be removed from log after delete");
    }

    // =========================================================================
    // Test 4: delete_deck (leader on someone else's deck) — success
    // =========================================================================

    [Test]
    public async Task DeleteDeck_by_leader_on_member_deck_succeeds()
    {
        using var factory = new SVSimTestFactory();
        long leaderId  = await factory.SeedViewerAsync(76_561_198_700_000_005UL, "AttLeader4");
        long memberId  = await factory.SeedViewerAsync(76_561_198_700_000_006UL, "AttMember4");

        await factory.SeedDeckAsync(memberId, Format.Rotation, 1, "MemberDeck");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        using var memberClient = factory.CreateAuthenticatedClient(memberId);

        await CreateGuildAsync(leaderClient, "AttGuild4");
        await AddMemberToGuildAsync(factory, leaderId, memberId, 0, "AttGuild4");

        var (addOk, _) = await AddDeckAsync(memberClient, deckFormat: 1, deckNo: 1);
        Assert.That(addOk, Is.True, "Member should be able to add deck");

        long messageId = await GetLastMessageIdAsync(leaderClient);

        // Leader deletes member's deck
        var (delOk, delBody) = await DeleteDeckAsync(leaderClient, deckFormat: 1, messageId);
        Assert.That(delOk, Is.True, $"Leader delete should succeed; got: {delBody}");
        using var doc = JsonDocument.Parse(delBody);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out _), Is.False,
            "Successful delete must NOT return result_code");
    }

    // =========================================================================
    // Test 5: delete_deck (regular member on someone else's deck) — rejected
    // =========================================================================

    [Test]
    public async Task DeleteDeck_by_regular_member_on_other_deck_rejected()
    {
        using var factory = new SVSimTestFactory();
        long leaderId  = await factory.SeedViewerAsync(76_561_198_700_000_007UL, "AttLeader5");
        long member1Id = await factory.SeedViewerAsync(76_561_198_700_000_008UL, "AttMember5a");
        long member2Id = await factory.SeedViewerAsync(76_561_198_700_000_009UL, "AttMember5b");

        await factory.SeedDeckAsync(member1Id, Format.Rotation, 1, "Member1Deck");

        using var leaderClient  = factory.CreateAuthenticatedClient(leaderId);
        using var member1Client = factory.CreateAuthenticatedClient(member1Id);
        using var member2Client = factory.CreateAuthenticatedClient(member2Id);

        await CreateGuildAsync(leaderClient, "AttGuild5");
        await AddMemberToGuildAsync(factory, leaderId, member1Id, 0, "AttGuild5");
        await AddMemberToGuildAsync(factory, leaderId, member2Id, 0, "AttGuild5");

        var (addOk, _) = await AddDeckAsync(member1Client, deckFormat: 1, deckNo: 1);
        Assert.That(addOk, Is.True, "Member1 should be able to add deck");

        long messageId = await GetLastMessageIdAsync(leaderClient);

        // Member2 tries to delete member1's deck — must fail
        var (delOk, delBody) = await DeleteDeckAsync(member2Client, deckFormat: 1, messageId);
        Assert.That(delOk, Is.True, "HTTP should be 200 even for logic rejection");
        using var doc = JsonDocument.Parse(delBody);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            "Rejected delete must return result_code");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), "result_code must be 2 for permission denied");
    }

    // =========================================================================
    // Test 6: add_replay — creates REPLAY row; replay_detail returns payload
    // =========================================================================

    [Test]
    public async Task AddReplay_creates_replay_message_and_replay_detail_returns_payload()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_700_000_010UL, "AttLeader6");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        await CreateGuildAsync(leaderClient, "AttGuild6");

        const long battleId = 999_888_777L;
        var (addOk, addBody) = await AddReplayAsync(leaderClient, battleId);
        Assert.That(addOk, Is.True, $"add_replay should return 200; got: {addBody}");

        // The REPLAY message should appear in the chat feed
        var msgResp = await leaderClient.PostAsync("/guild_chat/messages",
            JsonContent.Create(new
            {
                start_message_id = 0, direction = 1, wait_interval = 3,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var msgBody = await msgResp.Content.ReadAsStringAsync();
        using var msgDoc = JsonDocument.Parse(msgBody);
        var msgs = msgDoc.RootElement.GetProperty("chat_message");

        bool foundReplay = false;
        long replayMsgId = 0;
        for (int i = 0; i < msgs.GetArrayLength(); i++)
        {
            int msgType = GetStringifiedInt(msgs[i], "message_type");
            if (msgType == (int)GuildChatMessageType.Replay)
            {
                foundReplay = true;
                replayMsgId = GetStringifiedLong(msgs[i], "message_id");
            }
        }
        Assert.That(foundReplay, Is.True, "A Replay-type message must appear in chat");
        Assert.That(replayMsgId, Is.GreaterThan(0), "Replay message must have a valid message_id");

        // replay_detail should return the stored payload
        var (detailOk, detailRoot) = await ReplayDetailAsync(leaderClient, replayMsgId);
        Assert.That(detailOk, Is.True, "replay_detail should return 200");
        // Should NOT have result_code (error marker)
        Assert.That(detailRoot.TryGetProperty("result_code", out _), Is.False,
            "Successful replay_detail must not have result_code");
    }

    // =========================================================================
    // Test 7: deck_log — always-present Rotation/Unlimited/PreRotation buckets
    // =========================================================================

    [Test]
    public async Task DeckLog_always_returns_rotation_unlimited_prerotation_buckets()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_700_000_011UL, "AttLeader7");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        await CreateGuildAsync(leaderClient, "AttGuild7");

        var (ok, root) = await DeckLogAsync(leaderClient);
        Assert.That(ok, Is.True);
        Assert.That(root.TryGetProperty("deck_log", out var deckLogEl), Is.True);

        // Buckets "1", "2", "3" must always be present (even when empty)
        Assert.That(deckLogEl.TryGetProperty("1", out var rot), Is.True, "Rotation bucket '1' must always exist");
        Assert.That(deckLogEl.TryGetProperty("2", out var unl), Is.True, "Unlimited bucket '2' must always exist");
        Assert.That(deckLogEl.TryGetProperty("3", out var pre), Is.True, "PreRotation bucket '3' must always exist");

        Assert.That(rot.GetArrayLength(), Is.EqualTo(0), "Rotation bucket empty for fresh guild");
        Assert.That(unl.GetArrayLength(), Is.EqualTo(0), "Unlimited bucket empty");
        Assert.That(pre.GetArrayLength(), Is.EqualTo(0), "PreRotation bucket empty");

        // Crossover ("4") and MyRotation ("5") must NOT be present when empty
        Assert.That(deckLogEl.TryGetProperty("4", out _), Is.False, "Crossover bucket must be absent when empty");
        Assert.That(deckLogEl.TryGetProperty("5", out _), Is.False, "MyRotation bucket must be absent when empty");
    }

    // =========================================================================
    // Test 8: deck_log — multiple formats go into correct buckets
    // =========================================================================

    [Test]
    public async Task DeckLog_multiple_formats_go_into_correct_buckets()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_700_000_012UL, "AttLeader8");

        await factory.SeedDeckAsync(leaderId, Format.Rotation,    number: 1, name: "RotDeck");
        await factory.SeedDeckAsync(leaderId, Format.Unlimited,   number: 1, name: "UnlDeck");
        await factory.SeedDeckAsync(leaderId, Format.PreRotation, number: 1, name: "PreDeck");

        using var leaderClient = factory.CreateAuthenticatedClient(leaderId);
        await CreateGuildAsync(leaderClient, "AttGuild8");

        // Share all three
        await AddDeckAsync(leaderClient, deckFormat: 1, deckNo: 1); // Rotation
        await AddDeckAsync(leaderClient, deckFormat: 2, deckNo: 1); // Unlimited
        await AddDeckAsync(leaderClient, deckFormat: 3, deckNo: 1); // PreRotation

        var (ok, root) = await DeckLogAsync(leaderClient);
        Assert.That(ok, Is.True);
        var deckLogEl = root.GetProperty("deck_log");

        Assert.That(deckLogEl.GetProperty("1").GetArrayLength(), Is.EqualTo(1), "Rotation bucket has 1 entry");
        Assert.That(deckLogEl.GetProperty("2").GetArrayLength(), Is.EqualTo(1), "Unlimited bucket has 1 entry");
        Assert.That(deckLogEl.GetProperty("3").GetArrayLength(), Is.EqualTo(1), "PreRotation bucket has 1 entry");

        // Check deck names
        Assert.That(deckLogEl.GetProperty("1")[0].GetProperty("deck_name").GetString(), Is.EqualTo("RotDeck"));
        Assert.That(deckLogEl.GetProperty("2")[0].GetProperty("deck_name").GetString(), Is.EqualTo("UnlDeck"));
        Assert.That(deckLogEl.GetProperty("3")[0].GetProperty("deck_name").GetString(), Is.EqualTo("PreDeck"));
    }
}
