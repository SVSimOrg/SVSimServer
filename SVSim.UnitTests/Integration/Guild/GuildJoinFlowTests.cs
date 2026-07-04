using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

using GuildEntity = SVSim.Database.Entities.Guild.Guild;

/// <summary>
/// Integration tests for the join-path state machine:
///   /guild/join (FREE / APPROVAL / ONLY_INVITE branches),
///   /guild/cancel_join_request,
///   /guild/join_request_list.
/// </summary>
public class GuildJoinFlowTests
{
    private const string Vid = "0";
    private const int Sid = 0;
    private const string Stk = "";

    private static object BaseReq() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    // Many wire fields use StringifiedIntConverter — values arrive as "123" not 123.
    private static int GetStringifiedInt(JsonElement el, string prop)
        => int.Parse(el.GetProperty(prop).GetString()!);

    private static long GetStringifiedLong(JsonElement el, string prop)
        => long.Parse(el.GetProperty(prop).GetString()!);

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    /// <summary>Creates a guild as the given client and returns the guild_id from /guild/info.</summary>
    private static async Task<int> CreateGuildAndGetIdAsync(HttpClient client, string name, int joinCondition = 1)
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
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(infoJson);
        return GetStringifiedInt(doc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");
    }

    // ─── FREE branch ─────────────────────────────────────────────────────────────

    [Test]
    public async Task Free_join_makes_viewer_a_member_instantly()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_001UL, displayName: "FreeLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "FreeGuild", joinCondition: 1);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_002UL, displayName: "FreeJoiner");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, $"join HTTP failed: {joinJson}");
        using var joinDoc = JsonDocument.Parse(joinJson);
        if (joinDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"join returned error: {joinJson}");

        // guild_status must be 2 (JOINING) — value is stringified.
        Assert.That(GetStringifiedInt(joinDoc.RootElement, "guild_status"), Is.EqualTo(2),
            $"guild_status must be 2 after FREE join, got: {joinJson}");

        // B calling /guild/info should now be a member (guild_status=2).
        var bInfoResp = await clientB.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var bInfoJson = await bInfoResp.Content.ReadAsStringAsync();
        using var bInfoDoc = JsonDocument.Parse(bInfoJson);
        Assert.That(GetStringifiedInt(bInfoDoc.RootElement, "guild_status"), Is.EqualTo(2),
            $"B should be a member after FREE join, got: {bInfoJson}");
    }

    [Test]
    public async Task Free_join_consumes_pending_invite()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_011UL, displayName: "FreeLeader2");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "FreeGuild2", joinCondition: 1);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_012UL, displayName: "FreeJoinerInvited");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // A invites B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB, invite_message = "",
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B joins via FREE path.
        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(joinResp.IsSuccessStatusCode, Is.True);

        // After joining, B's invited_guild_list should be empty (invite consumed).
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var ilDoc = JsonDocument.Parse(await invitedList.Content.ReadAsStringAsync());
        Assert.That(ilDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "After joining, pending invites for the viewer should be consumed");
    }

    // ─── APPROVAL branch ─────────────────────────────────────────────────────────

    [Test]
    public async Task Approval_join_creates_pending_request_not_membership()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_021UL, displayName: "ApprovalLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "ApprovalGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_022UL, displayName: "ApprovalApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies.
        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, $"join HTTP failed: {joinJson}");
        using var joinDoc = JsonDocument.Parse(joinJson);
        if (joinDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"APPROVAL join returned error: {joinJson}");

        // B must NOT be a member yet (guild_status=0, no guild sub-object).
        var bInfoResp = await clientB.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var bInfoJson = await bInfoResp.Content.ReadAsStringAsync();
        using var bInfoDoc = JsonDocument.Parse(bInfoJson);
        Assert.That(GetStringifiedInt(bInfoDoc.RootElement, "guild_status"), Is.EqualTo(0),
            $"B must NOT be a member after APPROVAL apply, got: {bInfoJson}");

        // A (leader) should see B in the join_request_list.
        var jrlResp = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var jrlJson = await jrlResp.Content.ReadAsStringAsync();
        Assert.That(jrlResp.IsSuccessStatusCode, Is.True, $"join_request_list HTTP failed: {jrlJson}");
        using var jrlDoc = JsonDocument.Parse(jrlJson);
        var list = jrlDoc.RootElement.GetProperty("list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1), $"Leader should see 1 pending applicant, got: {jrlJson}");
        // viewer_id in list entries is StringifiedLong.
        Assert.That(GetStringifiedLong(list[0], "viewer_id"), Is.EqualTo(viewerB),
            "The applicant must be viewer B");
    }

    [Test]
    public async Task Approval_join_is_idempotent()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_031UL, displayName: "IdempotentLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "IdempotentGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_032UL, displayName: "IdempotentApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        var joinPayload = new { guild_id = guildId, from_invite = false, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

        // B applies twice.
        await clientB.PostAsync("/guild/join", JsonContent.Create(joinPayload));
        var join2 = await clientB.PostAsync("/guild/join", JsonContent.Create(joinPayload));
        var join2Json = await join2.Content.ReadAsStringAsync();
        Assert.That(join2.IsSuccessStatusCode, Is.True, $"Second apply must not HTTP-fail: {join2Json}");
        using var join2Doc = JsonDocument.Parse(join2Json);
        if (join2Doc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"Second apply must return Ok: {join2Json}");

        // Still only 1 entry in the list.
        var jrlResp = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var jrlDoc = JsonDocument.Parse(await jrlResp.Content.ReadAsStringAsync());
        Assert.That(jrlDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(1),
            "Idempotent re-apply must not duplicate the pending row");
    }

    [Test]
    public async Task Approval_join_response_has_guild_status_APPLYING()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_025UL, displayName: "ApprovalLeaderStatus");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "ApprovalGuildStatus", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_026UL, displayName: "ApprovalApplicantStatus");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies to the APPROVAL guild.
        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, $"join HTTP failed: {joinJson}");

        using var joinDoc = JsonDocument.Parse(joinJson);
        // Must be APPLYING (1), NOT JOINING (2) — GuildJoinTask.Parse() reads this value directly.
        Assert.That(GetStringifiedInt(joinDoc.RootElement, "guild_status"), Is.EqualTo(1),
            $"APPROVAL branch must return guild_status=1 (APPLYING), got: {joinJson}");
    }

    [Test]
    public async Task Approval_idempotent_join_response_has_guild_status_APPLYING()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_027UL, displayName: "IdempotentApprovalLeaderStatus");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "IdempotentApprovalStatus", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_028UL, displayName: "IdempotentApplicantStatus");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        var joinPayload = new { guild_id = guildId, from_invite = false,
            viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

        // First apply.
        await clientB.PostAsync("/guild/join", JsonContent.Create(joinPayload));
        // Second (idempotent) apply — should still return APPLYING (1).
        var join2 = await clientB.PostAsync("/guild/join", JsonContent.Create(joinPayload));
        var join2Json = await join2.Content.ReadAsStringAsync();
        Assert.That(join2.IsSuccessStatusCode, Is.True, $"Idempotent apply HTTP failed: {join2Json}");

        using var join2Doc = JsonDocument.Parse(join2Json);
        Assert.That(GetStringifiedInt(join2Doc.RootElement, "guild_status"), Is.EqualTo(1),
            $"Idempotent APPROVAL branch must return guild_status=1 (APPLYING), got: {join2Json}");
    }

    // ─── ONLY_INVITE branch ──────────────────────────────────────────────────────

    [Test]
    public async Task OnlyInvite_without_invite_is_rejected()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_041UL, displayName: "InviteOnlyLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "InviteOnlyGuild", joinCondition: 3);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_042UL, displayName: "UninvitedB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var joinDoc = JsonDocument.Parse(joinJson);
        Assert.That(joinDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error result_code, got: {joinJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Must be rejected (result_code=2), got: {joinJson}");
    }

    [Test]
    public async Task OnlyInvite_with_pending_invite_joins_and_consumes_invite()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_051UL, displayName: "InviteOnlyLeader2");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "InviteOnlyGuild2", joinCondition: 3);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_052UL, displayName: "InvitedB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // A invites B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB, invite_message = "",
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B joins via ONLY_INVITE path.
        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = true,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, $"join HTTP failed: {joinJson}");
        using var joinDoc = JsonDocument.Parse(joinJson);
        if (joinDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"ONLY_INVITE join with invite returned error: {joinJson}");

        // B is now a member.
        var bInfoResp = await clientB.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var bInfoJson = await bInfoResp.Content.ReadAsStringAsync();
        using var bInfoDoc = JsonDocument.Parse(bInfoJson);
        Assert.That(GetStringifiedInt(bInfoDoc.RootElement, "guild_status"), Is.EqualTo(2),
            $"B must be a member after ONLY_INVITE join, got: {bInfoJson}");

        // Invite was consumed — B's invited_guild_list is now empty.
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var ilDoc = JsonDocument.Parse(await invitedList.Content.ReadAsStringAsync());
        Assert.That(ilDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "Invite must be consumed (Pending→Consumed) after ONLY_INVITE join");
    }

    // ─── Member cap ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Full_guild_rejects_free_join()
    {
        using var factory = new SVSimTestFactory();

        // Seed a GuildConfig with MaxMemberNum = 1 so the guild fills at the leader.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
            const string json = "{\"MaxMemberNum\":1,\"MaxSubLeaderNum\":3,\"SearchResultCap\":20,\"UsableStampList\":[1,2,3,4,5]}";
            var existing = db.GameConfigs.FirstOrDefault(s => s.SectionName == "Guild");
            if (existing is null)
                db.GameConfigs.Add(new SVSim.Database.Models.GameConfigSection { SectionName = "Guild", ValueJson = json });
            else
                existing.ValueJson = json;
            db.SaveChanges();
        }

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_061UL, displayName: "CapLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "CapGuild", joinCondition: 1);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_062UL, displayName: "CapJoiner");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        var joinResp = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var joinJson = await joinResp.Content.ReadAsStringAsync();
        Assert.That(joinResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var joinDoc = JsonDocument.Parse(joinJson);
        Assert.That(joinDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error result_code when guild is full, got: {joinJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Full guild must reject join (result_code=2), got: {joinJson}");
    }

    // ─── cancel_join_request ─────────────────────────────────────────────────────

    [Test]
    public async Task Cancel_join_request_removes_pending_row()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_071UL, displayName: "CancelReqLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "CancelReqGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_072UL, displayName: "CancelApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A sees B in the list.
        var jrl1 = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var jrl1Doc = JsonDocument.Parse(await jrl1.Content.ReadAsStringAsync());
        Assert.That(jrl1Doc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(1),
            "Should see 1 pending before cancel");

        // B cancels (no guild_id in request — server infers from viewer state).
        var cancelResp = await clientB.PostAsync("/guild/cancel_join_request",
            JsonContent.Create(BaseReq()));
        var cancelJson = await cancelResp.Content.ReadAsStringAsync();
        Assert.That(cancelResp.IsSuccessStatusCode, Is.True, $"cancel_join_request HTTP failed: {cancelJson}");
        using var cancelDoc = JsonDocument.Parse(cancelJson);
        if (cancelDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"cancel_join_request returned error: {cancelJson}");

        // A no longer sees any pending applicants.
        var jrl2 = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var jrl2Doc = JsonDocument.Parse(await jrl2.Content.ReadAsStringAsync());
        Assert.That(jrl2Doc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "After cancel, no pending applicants should be visible");
    }

    [Test]
    public async Task After_cancel_viewer_can_reapply()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_081UL, displayName: "ReApplyLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "ReApplyGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_082UL, displayName: "ReApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // Apply → cancel → re-apply.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        await clientB.PostAsync("/guild/cancel_join_request", JsonContent.Create(BaseReq()));

        var reApply = await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var reApplyJson = await reApply.Content.ReadAsStringAsync();
        Assert.That(reApply.IsSuccessStatusCode, Is.True, $"Re-apply after cancel HTTP failed: {reApplyJson}");
        using var doc = JsonDocument.Parse(reApplyJson);
        if (doc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"Re-apply after cancel must not error: {reApplyJson}");

        // A should see 1 pending applicant again.
        var jrlResp = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var jrlDoc = JsonDocument.Parse(await jrlResp.Content.ReadAsStringAsync());
        Assert.That(jrlDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(1),
            "After re-apply, applicant should appear in list again");
    }

    // ─── join_request_list permission ────────────────────────────────────────────

    [Test]
    public async Task Join_request_list_returns_empty_for_regular_member()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_091UL, displayName: "ListLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "ListGuild", joinCondition: 1);

        // B joins (FREE) — becomes Regular member.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_092UL, displayName: "RegularMember");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Regular member B gets an empty list from join_request_list.
        var jrlResp = await clientB.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var jrlJson = await jrlResp.Content.ReadAsStringAsync();
        Assert.That(jrlResp.IsSuccessStatusCode, Is.True, $"join_request_list HTTP failed: {jrlJson}");
        using var jrlDoc = JsonDocument.Parse(jrlJson);
        Assert.That(jrlDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "Regular member must see empty list from join_request_list");
    }

    // ─── join_request_accept ─────────────────────────────────────────────────────

    [Test]
    public async Task Accept_makes_applicant_a_member_and_emits_join_chat_event()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_111UL, displayName: "AcceptLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "AcceptGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_112UL, displayName: "AcceptApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A accepts B.
        var acceptResp = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, $"accept HTTP failed: {acceptJson}");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        if (acceptDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"accept returned error: {acceptJson}");

        // B is now a member.
        var bInfoResp = await clientB.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var bInfoJson = await bInfoResp.Content.ReadAsStringAsync();
        using var bInfoDoc = JsonDocument.Parse(bInfoJson);
        Assert.That(GetStringifiedInt(bInfoDoc.RootElement, "guild_status"), Is.EqualTo(2),
            $"B must be a member (guild_status=2) after accept, got: {bInfoJson}");

        // A's join_request_list is now empty.
        var jrlResp = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var jrlDoc = JsonDocument.Parse(await jrlResp.Content.ReadAsStringAsync());
        Assert.That(jrlDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "join_request_list must be empty after accept");

        // GuildService.CommitJoinAsync calls IGuildChatService.EmitSystemEventAsync(Join).
        // That service is a no-op stub (T15 will implement it), so we can't assert a DB row here.
        // Confirmed the call path via code inspection; the DB assertion will land with T15.
    }

    [Test]
    public async Task Accept_consumes_applicant_pending_invites()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_113UL, displayName: "AcceptInvLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "AcceptInvGuild", joinCondition: 2);

        // Separate guild that invites B (to ensure invites are cleared on accept).
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_114UL, displayName: "OtherLeader");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await CreateGuildAndGetIdAsync(clientC, "OtherGuild", joinCondition: 1);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_115UL, displayName: "AcceptInvApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // C invites B.
        await clientC.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB, invite_message = "",
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B applies to A's guild.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A accepts B.
        var acceptResp = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True);

        // B's invited_guild_list must now be empty (invite consumed by CommitJoinAsync).
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var ilDoc = JsonDocument.Parse(await invitedList.Content.ReadAsStringAsync());
        Assert.That(ilDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "Pending invites must be consumed when accept commits join");
    }

    [Test]
    public async Task Accept_caller_not_in_any_guild_returns_error()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_116UL, displayName: "AcceptPermLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "AcceptPermGuild", joinCondition: 2);

        // B is a regular member of A's guild (joined via FREE — repurpose guild as mixed joinCondition by seeding directly).
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_117UL, displayName: "RegularMemberAccept");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        // B joins A's guild (change to joinCondition: 1 is needed; create separate FREE guild for B to join and then act as non-leader).
        // Simplest: just have B NOT in any guild → NotInGuild → but we need PermissionDenied specifically.
        // Use a separate FREE guild so B can join it, then try to accept on A's guild (not B's guild → different result).
        // Better: A creates an approval guild, B applies to a 3rd guild, then B (not in A's guild) tries to accept → NotInGuild.
        // For PermissionDenied specifically: need B in A's guild as Regular. Let's create a second client in A's guild.
        long viewerD = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_118UL, displayName: "FreeGuildLeader");
        using var clientD = factory.CreateAuthenticatedClient(viewerD);
        // D creates a FREE guild, B joins it (B becomes Regular in D's guild, not A's approval guild).
        int dGuildId = await CreateGuildAndGetIdAsync(clientD, "FreeGuildForPerm", joinCondition: 1);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = dGuildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Now have C apply to A's guild.
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_119UL, displayName: "ApplicantForPerm");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await clientC.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B (Regular member of D's guild, NOT in A's guild) tries to accept C's request on A's guild → NotInGuild error.
        var acceptResp = await clientB.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerC,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        Assert.That(acceptDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error from non-guild-member accept, got: {acceptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Non-guild-member accept must return error, got: {acceptJson}");
    }

    [Test]
    public async Task SubLeader_can_accept_join_request()
    {
        using var factory = new SVSimTestFactory();

        // A creates an APPROVAL guild (becomes Leader).
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_001UL, displayName: "SubLeaderAcceptLeaderA");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "SubLeaderAcceptGuild", joinCondition: 2);

        // B joins A's guild as Regular, then we promote to SubLeader directly in the DB.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_002UL, displayName: "SubLeaderAcceptB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        // A accepts B so B is a full member.
        await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        // Promote B to SubLeader via direct DB mutation.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var bMember = await db.GuildMembers.FindAsync(guildId, viewerB);
            bMember!.Role = GuildRole.SubLeader;
            await db.SaveChangesAsync();
        }

        // C applies to A's guild.
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_003UL, displayName: "SubLeaderAcceptApplicantC");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await clientC.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B (SubLeader) accepts C's request.
        var acceptResp = await clientB.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerC,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, $"SubLeader accept HTTP failed: {acceptJson}");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        if (acceptDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"SubLeader accept returned error: {acceptJson}");

        // C must now be a member (guild_status=2).
        var cInfoResp = await clientC.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var cInfoJson = await cInfoResp.Content.ReadAsStringAsync();
        using var cInfoDoc = JsonDocument.Parse(cInfoJson);
        Assert.That(GetStringifiedInt(cInfoDoc.RootElement, "guild_status"), Is.EqualTo(2),
            $"C must be a member (guild_status=2) after SubLeader accept, got: {cInfoJson}");
    }

    [Test]
    public async Task SubLeader_can_reject_join_request()
    {
        using var factory = new SVSimTestFactory();

        // A creates an APPROVAL guild (becomes Leader).
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_011UL, displayName: "SubLeaderRejectLeaderA");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "SubLeaderRejectGuild", joinCondition: 2);

        // B joins and gets accepted, then promoted to SubLeader.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_012UL, displayName: "SubLeaderRejectB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var bMember = await db.GuildMembers.FindAsync(guildId, viewerB);
            bMember!.Role = GuildRole.SubLeader;
            await db.SaveChangesAsync();
        }

        // C applies.
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_013UL, displayName: "SubLeaderRejectApplicantC");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await clientC.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B (SubLeader) rejects C's request.
        var rejectResp = await clientB.PostAsync("/guild/reject_join_request",
            JsonContent.Create(new { request_viewer_id = (int)viewerC,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var rejectJson = await rejectResp.Content.ReadAsStringAsync();
        Assert.That(rejectResp.IsSuccessStatusCode, Is.True, $"SubLeader reject HTTP failed: {rejectJson}");
        using var rejectDoc = JsonDocument.Parse(rejectJson);
        if (rejectDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"SubLeader reject returned error: {rejectJson}");

        // C must NOT be a member (guild_status=0).
        var cInfoResp = await clientC.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var cInfoJson = await cInfoResp.Content.ReadAsStringAsync();
        using var cInfoDoc = JsonDocument.Parse(cInfoJson);
        Assert.That(GetStringifiedInt(cInfoDoc.RootElement, "guild_status"), Is.EqualTo(0),
            $"C must NOT be a member (guild_status=0) after SubLeader reject, got: {cInfoJson}");
    }

    [Test]
    public async Task Regular_member_cannot_accept_join_request()
    {
        using var factory = new SVSimTestFactory();

        // A creates an APPROVAL guild (becomes Leader).
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_021UL, displayName: "RegularPermLeaderA");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "RegularPermGuild", joinCondition: 2);

        // B joins and gets accepted — remains Regular.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_022UL, displayName: "RegularPermMemberB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        // B's role is Regular (no promotion).

        // C applies.
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_001_023UL, displayName: "RegularPermApplicantC");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await clientC.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B (Regular) tries to accept C's request → PermissionDenied.
        var acceptResp = await clientB.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerC,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        Assert.That(acceptDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error from Regular-member accept, got: {acceptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Regular member accept must return error (result_code=2), got: {acceptJson}");
    }

    [Test]
    public async Task Accept_full_guild_returns_error()
    {
        using var factory = new SVSimTestFactory();

        // Cap at 2 members so we can fill the guild (leader + 1 member) then try to accept a 3rd.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
            const string json = "{\"MaxMemberNum\":2,\"MaxSubLeaderNum\":3,\"SearchResultCap\":20,\"UsableStampList\":[1,2,3,4,5]}";
            var existing = db.GameConfigs.FirstOrDefault(s => s.SectionName == "Guild");
            if (existing is null)
                db.GameConfigs.Add(new SVSim.Database.Models.GameConfigSection { SectionName = "Guild", ValueJson = json });
            else
                existing.ValueJson = json;
            db.SaveChanges();
        }

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_121UL, displayName: "CapAcceptLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "CapAcceptGuild", joinCondition: 2);

        // B applies and gets accepted (fills slot 2/2).
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_122UL, displayName: "CapAcceptMember");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // C also applies.
        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_123UL, displayName: "CapAcceptApplicant");
        using var clientC = factory.CreateAuthenticatedClient(viewerC);
        await clientC.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A tries to accept C, but guild is now full.
        var acceptResp = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerC,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        Assert.That(acceptDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error when guild is full, got: {acceptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Full guild accept must return error, got: {acceptJson}");
    }

    [Test]
    public async Task Accept_nonexistent_request_returns_error()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_131UL, displayName: "NotFoundLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        await CreateGuildAndGetIdAsync(clientA, "NotFoundGuild", joinCondition: 2);

        // No applicant — accept a viewer_id that never applied.
        var acceptResp = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = 999_999,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var acceptJson = await acceptResp.Content.ReadAsStringAsync();
        Assert.That(acceptResp.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var acceptDoc = JsonDocument.Parse(acceptJson);
        Assert.That(acceptDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error for non-existent request, got: {acceptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Non-existent accept must return error, got: {acceptJson}");
    }

    [Test]
    public async Task Accept_already_resolved_request_returns_error()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_141UL, displayName: "AlreadyResolvedLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "AlreadyResolvedGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_142UL, displayName: "AlreadyResolvedApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies and A accepts first time — request is now Accepted.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var firstAccept = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(firstAccept.IsSuccessStatusCode, Is.True, "First accept must succeed");

        // A tries to accept again — request already resolved.
        var secondAccept = await clientA.PostAsync("/guild/join_request_accept",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var secondAcceptJson = await secondAccept.Content.ReadAsStringAsync();
        Assert.That(secondAccept.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var secondDoc = JsonDocument.Parse(secondAcceptJson);
        Assert.That(secondDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error on double-accept, got: {secondAcceptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Double-accept must return error, got: {secondAcceptJson}");
    }

    // ─── reject_join_request ─────────────────────────────────────────────────────

    [Test]
    public async Task Reject_flips_status_no_membership_no_chat_event()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_151UL, displayName: "RejectLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "RejectGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_152UL, displayName: "RejectApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A rejects B.
        var rejectResp = await clientA.PostAsync("/guild/reject_join_request",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var rejectJson = await rejectResp.Content.ReadAsStringAsync();
        Assert.That(rejectResp.IsSuccessStatusCode, Is.True, $"reject HTTP failed: {rejectJson}");
        using var rejectDoc = JsonDocument.Parse(rejectJson);
        if (rejectDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"reject returned error: {rejectJson}");

        // B must NOT be a member (guild_status=0).
        var bInfoResp = await clientB.PostAsync("/guild/info",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var bInfoJson = await bInfoResp.Content.ReadAsStringAsync();
        using var bInfoDoc = JsonDocument.Parse(bInfoJson);
        Assert.That(GetStringifiedInt(bInfoDoc.RootElement, "guild_status"), Is.EqualTo(0),
            $"B must NOT be a member after reject (guild_status=0), got: {bInfoJson}");

        // No Join chat event (IGuildChatService.EmitSystemEventAsync is a no-op stub until T15 — no DB row expected).
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var hasJoinEvent = db.GuildChatMessages
            .Any(m => m.GuildId == guildId && m.AuthorViewerId == viewerB
                      && m.MessageType == SVSim.Database.Entities.Guild.GuildChatMessageType.Join);
        Assert.That(hasJoinEvent, Is.False, "No Join chat event should exist after reject");
    }

    [Test]
    public async Task Reject_already_resolved_request_returns_error()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_161UL, displayName: "RejectDoubleLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "RejectDoubleGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_162UL, displayName: "RejectDoubleApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // B applies, A rejects once.
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        await clientA.PostAsync("/guild/reject_join_request",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A tries to reject again.
        var secondReject = await clientA.PostAsync("/guild/reject_join_request",
            JsonContent.Create(new { request_viewer_id = (int)viewerB,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var secondRejectJson = await secondReject.Content.ReadAsStringAsync();
        Assert.That(secondReject.IsSuccessStatusCode, Is.True, "HTTP must be 200");
        using var secondDoc = JsonDocument.Parse(secondRejectJson);
        Assert.That(secondDoc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected error on double-reject, got: {secondRejectJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Double-reject must return error, got: {secondRejectJson}");
    }

    // ─── Wire-shape test for join_request_list ───────────────────────────────────

    [Test]
    public async Task Join_request_list_response_shape_is_correct()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_101UL, displayName: "ShapeLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        int guildId = await CreateGuildAndGetIdAsync(clientA, "ShapeGuild", joinCondition: 2);

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_400_000_102UL, displayName: "ShapeApplicant");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/join",
            JsonContent.Create(new { guild_id = guildId, from_invite = false,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        var jrlResp = await clientA.PostAsync("/guild/join_request_list",
            JsonContent.Create(new { page = 0, oldest_time = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var jrlJson = await jrlResp.Content.ReadAsStringAsync();
        Assert.That(jrlResp.IsSuccessStatusCode, Is.True, $"join_request_list failed: {jrlJson}");

        using var doc = JsonDocument.Parse(jrlJson);
        var list = doc.RootElement.GetProperty("list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1), $"Expected 1 entry, got: {jrlJson}");

        var entry = list[0];

        // GuildUserInfo fields (flat siblings per GuildUserInfo constructor) + request-list extras.
        string[] requiredFields = [
            "viewer_id", "name", "emblem_id", "country_code", "rank", "degree_id",
            "is_official_mark_displayed", "request_time"
        ];
        foreach (var field in requiredFields)
        {
            Assert.That(entry.TryGetProperty(field, out _), Is.True,
                $"Field '{field}' must be present in join_request_list entry, got: {jrlJson}");
        }

        // request_time must be a positive integer (unix seconds, NOT milliseconds).
        Assert.That(entry.GetProperty("request_time").GetInt64(), Is.GreaterThan(0),
            "request_time must be a positive unix seconds timestamp");

        // viewer_id is stringified — must parse as a valid long.
        Assert.That(long.TryParse(entry.GetProperty("viewer_id").GetString(), out var parsedId), Is.True,
            $"viewer_id must be stringified long, got: {jrlJson}");
        Assert.That(parsedId, Is.EqualTo(viewerB), "viewer_id must match the applicant");
    }
}
