using System.Net.Http.Json;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

using GuildEntity = SVSim.Database.Entities.Guild.Guild;

/// <summary>
/// Integration tests for the 5 guild invite endpoints:
///   /guild/invite, /guild/cancel_invite, /guild/reject_invite,
///   /guild/invite_user_list, /guild/invited_guild_list.
/// </summary>
public class GuildInviteFlowTests
{
    // Shared base-request fields (test mode — always zeros)
    private const string Vid = "0";
    private const int Sid = 0;
    private const string Stk = "";

    private static object BaseReq() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    // ─── Happy-path: A invites B → B sees it → A sees outgoing ────────────────

    [Test]
    public async Task Invite_then_invited_guild_list_shows_pending_entry()
    {
        using var factory = new SVSimTestFactory();

        // Viewer A: leader of a guild.
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_001UL, displayName: "InviterA");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);

        // Create guild as A.
        var create = await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "InviteTestGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(create.IsSuccessStatusCode, Is.True, $"create failed: {await create.Content.ReadAsStringAsync()}");

        // Viewer B: not in any guild.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_002UL, displayName: "InviteeB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // A invites B.
        var invite = await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var inviteJson = await invite.Content.ReadAsStringAsync();
        Assert.That(invite.IsSuccessStatusCode, Is.True, $"invite failed: {inviteJson}");
        using var inviteDoc = JsonDocument.Parse(inviteJson);
        if (inviteDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"invite returned error: {inviteJson}");

        // B should see pending invite in /guild/invited_guild_list.
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var invitedJson = await invitedList.Content.ReadAsStringAsync();
        Assert.That(invitedList.IsSuccessStatusCode, Is.True, $"invited_guild_list failed: {invitedJson}");

        using var invitedDoc = JsonDocument.Parse(invitedJson);
        var list = invitedDoc.RootElement.GetProperty("list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1), $"B should see exactly 1 pending invite, got: {invitedJson}");

        var entry = list[0];
        Assert.That(entry.GetProperty("guild_name").GetString(), Is.EqualTo("InviteTestGuild"),
            "guild_name must match the created guild");
        Assert.That(entry.TryGetProperty("invite_id", out var inviteIdProp), Is.True,
            "invite_id must be present in the response");
        Assert.That(inviteIdProp.GetInt64(), Is.GreaterThan(0), "invite_id must be a positive integer");

        // Wire-shape check: flat siblings (no nesting under 'detail')
        Assert.That(entry.TryGetProperty("guild_id", out _), Is.True, "guild_id must be flat sibling");
        Assert.That(entry.TryGetProperty("leader_name", out _), Is.True, "leader_name must be flat sibling");
    }

    [Test]
    public async Task Invite_user_list_shows_outgoing_invite_with_correct_shape()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_011UL, displayName: "LeaderA");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);

        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "OutgoingTestGuild", activity = 2, join_condition = 2,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_012UL, displayName: "InviteeB2");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // A invites B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A checks invite_user_list — should see B.
        var userList = await clientA.PostAsync("/guild/invite_user_list",
            JsonContent.Create(BaseReq()));
        var userListJson = await userList.Content.ReadAsStringAsync();
        Assert.That(userList.IsSuccessStatusCode, Is.True, $"invite_user_list failed: {userListJson}");

        using var doc = JsonDocument.Parse(userListJson);
        var list = doc.RootElement.GetProperty("list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1), $"Should see 1 outgoing invite, got: {userListJson}");

        var entry = list[0];
        // Wire-shape: GuildUserInfo fields + invite_id + invite_time as flat siblings
        Assert.That(entry.TryGetProperty("viewer_id", out _), Is.True, "viewer_id must be present");
        Assert.That(entry.TryGetProperty("name", out _), Is.True, "name must be present");
        Assert.That(entry.TryGetProperty("invite_id", out var invId), Is.True, "invite_id must be present");
        Assert.That(invId.GetInt64(), Is.GreaterThan(0), "invite_id must be positive");
        Assert.That(entry.TryGetProperty("invite_time", out var invTime), Is.True, "invite_time must be present");
        Assert.That(invTime.GetInt64(), Is.GreaterThan(0), "invite_time must be positive unix timestamp");
    }

    // ─── Cancel path ────────────────────────────────────────────────────────────

    [Test]
    public async Task Cancel_invite_removes_pending_from_invitee_list()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_021UL, displayName: "CancelLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);

        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "CancelTestGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_022UL, displayName: "CancelInvitee");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // Invite B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Retrieve the invite_id from invite_user_list.
        var userList = await clientA.PostAsync("/guild/invite_user_list", JsonContent.Create(BaseReq()));
        using var ulDoc = JsonDocument.Parse(await userList.Content.ReadAsStringAsync());
        long inviteId = ulDoc.RootElement.GetProperty("list")[0].GetProperty("invite_id").GetInt64();

        // A cancels.
        var cancel = await clientA.PostAsync("/guild/cancel_invite",
            JsonContent.Create(new { invite_id = inviteId, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var cancelJson = await cancel.Content.ReadAsStringAsync();
        Assert.That(cancel.IsSuccessStatusCode, Is.True, $"cancel_invite HTTP failed: {cancelJson}");
        using var cancelDoc = JsonDocument.Parse(cancelJson);
        if (cancelDoc.RootElement.TryGetProperty("result_code", out var crc))
            Assert.That(crc.GetInt32(), Is.Not.EqualTo(2), $"cancel_invite returned error: {cancelJson}");

        // B's invited_guild_list should now be empty.
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var ilDoc = JsonDocument.Parse(await invitedList.Content.ReadAsStringAsync());
        Assert.That(ilDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "After cancel, invitee should see 0 pending invites");
    }

    // ─── Reject path ────────────────────────────────────────────────────────────

    [Test]
    public async Task Reject_invite_removes_pending_from_invitee_list()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_031UL, displayName: "RejectLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);

        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "RejectTestGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_032UL, displayName: "RejectInvitee");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // A invites B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Get invite_id from B's invited_guild_list.
        var invitedList = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var ilDoc = JsonDocument.Parse(await invitedList.Content.ReadAsStringAsync());
        long inviteId = ilDoc.RootElement.GetProperty("list")[0].GetProperty("invite_id").GetInt64();

        // B rejects.
        var reject = await clientB.PostAsync("/guild/reject_invite",
            JsonContent.Create(new { invite_id = inviteId, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var rejectJson = await reject.Content.ReadAsStringAsync();
        Assert.That(reject.IsSuccessStatusCode, Is.True, $"reject_invite HTTP failed: {rejectJson}");
        using var rejectDoc = JsonDocument.Parse(rejectJson);
        if (rejectDoc.RootElement.TryGetProperty("result_code", out var rrc))
            Assert.That(rrc.GetInt32(), Is.Not.EqualTo(2), $"reject_invite returned error: {rejectJson}");

        // B's list should now be empty.
        var afterReject = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var arDoc = JsonDocument.Parse(await afterReject.Content.ReadAsStringAsync());
        Assert.That(arDoc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(0),
            "After reject, invitee should see 0 pending invites");
    }

    // ─── Permission: regular member cannot invite ────────────────────────────

    [Test]
    public async Task Regular_member_cannot_invite()
    {
        using var factory = new SVSimTestFactory();

        // A creates guild (becomes leader).
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_041UL, displayName: "PermLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "PermTestGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B: not in any guild — tries to invite C (but B is not leader/subleader of any guild).
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_042UL, displayName: "RegularB");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        long viewerC = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_043UL, displayName: "TargetC");

        // B tries to invite C (B has no guild — should get PermissionDenied / error).
        var inviteAttempt = await clientB.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerC,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var attemptJson = await inviteAttempt.Content.ReadAsStringAsync();
        Assert.That(inviteAttempt.IsSuccessStatusCode, Is.True, "HTTP level must still be 200");
        using var doc = JsonDocument.Parse(attemptJson);
        Assert.That(doc.RootElement.TryGetProperty("result_code", out var rc), Is.True,
            $"Expected result_code error, got: {attemptJson}");
        Assert.That(rc.GetInt32(), Is.EqualTo(2), $"Expected error code 2, got: {attemptJson}");
    }

    // ─── Re-invite after reject / cancel (partial-index regression) ─────────

    [Test]
    public async Task Reinvite_after_reject_succeeds()
    {
        using var factory = new SVSimTestFactory();

        // A creates guild and becomes leader.
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_061UL, displayName: "ReinviteLeader1");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "ReinviteRejectGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B: not in any guild.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_062UL, displayName: "ReinviteeB1");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // First invite: A → B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B rejects.
        var list1 = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var l1Doc = JsonDocument.Parse(await list1.Content.ReadAsStringAsync());
        long inviteId1 = l1Doc.RootElement.GetProperty("list")[0].GetProperty("invite_id").GetInt64();

        await clientB.PostAsync("/guild/reject_invite",
            JsonContent.Create(new { invite_id = inviteId1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Second invite: A → B again (was crashing before partial index).
        var reinvite = await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var reinviteJson = await reinvite.Content.ReadAsStringAsync();
        Assert.That(reinvite.IsSuccessStatusCode, Is.True, $"Re-invite after reject HTTP failed: {reinviteJson}");
        using var riDoc = JsonDocument.Parse(reinviteJson);
        if (riDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.EqualTo(1), $"Re-invite after reject must return result_code=1, got: {reinviteJson}");

        // B should see a new pending invite.
        var list2 = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var l2Doc = JsonDocument.Parse(await list2.Content.ReadAsStringAsync());
        Assert.That(l2Doc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(1),
            "After re-invite post-reject, invitee should see exactly 1 pending invite");
    }

    [Test]
    public async Task Reinvite_after_cancel_succeeds()
    {
        using var factory = new SVSimTestFactory();

        // A creates guild and becomes leader.
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_071UL, displayName: "ReinviteLeader2");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "ReinviteCancelGuild", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // B: not in any guild.
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_072UL, displayName: "ReinviteeB2");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        // First invite: A → B.
        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // A cancels.
        var ulResp = await clientA.PostAsync("/guild/invite_user_list", JsonContent.Create(BaseReq()));
        using var ulDoc = JsonDocument.Parse(await ulResp.Content.ReadAsStringAsync());
        long inviteId1 = ulDoc.RootElement.GetProperty("list")[0].GetProperty("invite_id").GetInt64();

        await clientA.PostAsync("/guild/cancel_invite",
            JsonContent.Create(new { invite_id = inviteId1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        // Second invite: A → B again (was crashing before partial index).
        var reinvite = await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var reinviteJson = await reinvite.Content.ReadAsStringAsync();
        Assert.That(reinvite.IsSuccessStatusCode, Is.True, $"Re-invite after cancel HTTP failed: {reinviteJson}");
        using var riDoc = JsonDocument.Parse(reinviteJson);
        if (riDoc.RootElement.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.EqualTo(1), $"Re-invite after cancel must return result_code=1, got: {reinviteJson}");

        // B should see a new pending invite.
        var list2 = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        using var l2Doc = JsonDocument.Parse(await list2.Content.ReadAsStringAsync());
        Assert.That(l2Doc.RootElement.GetProperty("list").GetArrayLength(), Is.EqualTo(1),
            "After re-invite post-cancel, invitee should see exactly 1 pending invite");
    }

    // ─── Wire-shape literal JSON test for invited_guild_list ─────────────────

    [Test]
    public async Task InvitedGuildList_response_shape_is_flat_siblings_not_nested()
    {
        using var factory = new SVSimTestFactory();

        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_051UL, displayName: "ShapeLeader");
        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        await clientA.PostAsync("/guild/create",
            JsonContent.Create(new { guild_name = "ShapeGuild", activity = 3, join_condition = 3,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_300_000_052UL, displayName: "ShapeInvitee");
        using var clientB = factory.CreateAuthenticatedClient(viewerB);

        await clientA.PostAsync("/guild/invite",
            JsonContent.Create(new { invited_viewer_id = (int)viewerB,
                invite_message = "", viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));

        var resp = await clientB.PostAsync("/guild/invited_guild_list",
            JsonContent.Create(new { page = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement.GetProperty("list")[0];

        // GuildDetailInfo fields must be FLAT siblings of invite_id (not nested under 'detail' or similar).
        // This matches GuildInvitedListTask.Parse() which calls new GuildDetailInfo(json) and reads
        // invite_id from the SAME json node.
        string[] required = [
            "guild_id", "guild_name", "description", "guild_emblem_id",
            "join_condition", "activity", "member_num", "leader_name", "leader_viewer_id",
            "invite_id"
        ];
        foreach (var field in required)
        {
            Assert.That(entry.TryGetProperty(field, out _), Is.True,
                $"Field '{field}' must be a flat sibling in /guild/invited_guild_list response, got: {json}");
        }

        // Verify no nesting — there must NOT be a 'detail' sub-object.
        Assert.That(entry.TryGetProperty("detail", out _), Is.False,
            $"'detail' wrapper must NOT be present; fields must be flat. Response: {json}");
    }
}
