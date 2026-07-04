using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

/// <summary>
/// Integration tests for:
///   /guild/change_role — full HTTP round-trip, response shape, atomic transfer
///   /guild/friend_list — response shape with is_join_guild annotation
/// </summary>
public class GuildChangeRoleFriendListFlowTests
{
    private const string Vid = "0";
    private const int Sid = 0;
    private const string Stk = "";

    private static object BaseReq() => new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

    private static int GetStringifiedInt(JsonElement el, string prop)
        => int.Parse(el.GetProperty(prop).GetString()!);

    private static long GetStringifiedLong(JsonElement el, string prop)
        => long.Parse(el.GetProperty(prop).GetString()!);

    // ─── Helpers ─────────────────────────────────────────────────────────────────

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
            JsonContent.Create(BaseReq()));
        var infoJson = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(infoJson);
        return GetStringifiedInt(doc.RootElement.GetProperty("guild").GetProperty("detail"), "guild_id");
    }

    private static async Task AddMemberDirectlyAsync(SVSimTestFactory factory, int guildId, long viewerId, GuildRole role)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.GuildMembers.Add(new GuildMember
        {
            GuildId  = guildId,
            ViewerId = viewerId,
            Role     = role,
            JoinedAt = DateTime.UtcNow,
        });
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.GuildId = guildId;
        await db.SaveChangesAsync();
    }

    // ─── /guild/change_role ───────────────────────────────────────────────────────

    [Test]
    public async Task ChangeRole_leader_promotes_regular_returns_full_member_list()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_001UL, "CrfLeader1");
        long memberId = await factory.SeedViewerAsync(76_561_198_500_000_002UL, "CrfMember1");

        using var clientLeader = factory.CreateAuthenticatedClient(leaderId);
        int guildId = await CreateGuildAndGetIdAsync(clientLeader, "CrfGuild1");
        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        // POST /guild/change_role — promote memberId to SubLeader (role_id=2)
        var resp = await clientLeader.PostAsync("/guild/change_role",
            JsonContent.Create(new
            {
                target_viewer_id = memberId,
                role_id = 2,       // SubLeader
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True, $"HTTP failed: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Must not be a wire error.
        if (root.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"change_role returned error: {body}");

        // Response must include "members" array.
        Assert.That(root.TryGetProperty("members", out var members), Is.True,
            $"response must have 'members' array: {body}");
        Assert.That(members.ValueKind, Is.EqualTo(JsonValueKind.Array));

        // Should have 2 members (leader + promoted member).
        Assert.That(members.GetArrayLength(), Is.EqualTo(2),
            "members list must contain both leader and the promoted member");

        // Find the promoted member entry.
        var promoted = members.EnumerateArray()
            .FirstOrDefault(m => GetStringifiedLong(m, "viewer_id") == memberId);
        Assert.That(promoted.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined),
            "Promoted member must be in the members list");
        Assert.That(GetStringifiedInt(promoted, "role"), Is.EqualTo(2),
            "Promoted member's role must be 2 (SubLeader)");

        // viewer_id must be stringified.
        Assert.That(promoted.GetProperty("viewer_id").ValueKind, Is.EqualTo(JsonValueKind.String),
            "viewer_id in members must be a string (stringified)");
    }

    [Test]
    public async Task ChangeRole_atomic_leader_transfer_response_reflects_new_roles()
    {
        using var factory = new SVSimTestFactory();
        long leaderId = await factory.SeedViewerAsync(76_561_198_500_000_003UL, "CrfLeader2");
        long targetId = await factory.SeedViewerAsync(76_561_198_500_000_004UL, "CrfTarget2");

        using var clientLeader = factory.CreateAuthenticatedClient(leaderId);
        int guildId = await CreateGuildAndGetIdAsync(clientLeader, "CrfGuild2");
        await AddMemberDirectlyAsync(factory, guildId, targetId, GuildRole.Regular);

        // Transfer leadership (role_id=1).
        var resp = await clientLeader.PostAsync("/guild/change_role",
            JsonContent.Create(new
            {
                target_viewer_id = targetId,
                role_id = 1,       // Leader
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True, $"HTTP failed: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("result_code", out var rc))
            Assert.That(rc.GetInt32(), Is.Not.EqualTo(2), $"change_role returned error: {body}");

        // Both members must be in the response.
        Assert.That(root.TryGetProperty("members", out var members), Is.True);
        Assert.That(members.GetArrayLength(), Is.EqualTo(2));

        var newLeaderEntry = members.EnumerateArray()
            .FirstOrDefault(m => GetStringifiedLong(m, "viewer_id") == targetId);
        Assert.That(GetStringifiedInt(newLeaderEntry, "role"), Is.EqualTo(1), "Target must have role=1 (Leader)");

        var formerLeaderEntry = members.EnumerateArray()
            .FirstOrDefault(m => GetStringifiedLong(m, "viewer_id") == leaderId);
        Assert.That(GetStringifiedInt(formerLeaderEntry, "role"), Is.EqualTo(0), "Former leader must have role=0 (Regular)");
    }

    // ─── /guild/friend_list ───────────────────────────────────────────────────────

    [Test]
    public async Task FriendList_returns_bare_array_with_is_join_guild_field()
    {
        // GuildFriendListTask.Parse() reads base.ResponseData["data"][i] directly — data is a bare array.
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(76_561_198_500_000_005UL, "CrfViewerA");
        long viewerB = await factory.SeedViewerAsync(76_561_198_500_000_006UL, "CrfViewerB");

        // Make viewerB a friend of viewerA by inserting both directions directly.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerA, FriendViewerId = viewerB, CreatedAt = DateTime.UtcNow });
            db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerB, FriendViewerId = viewerA, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        var resp = await clientA.PostAsync("/guild/friend_list", JsonContent.Create(BaseReq()));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True, $"HTTP failed: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Response must be a bare JSON array at root — NOT an object with a "friends" key.
        Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Array),
            $"friend_list data must be a bare array at root: {body}");
        Assert.That(root.GetArrayLength(), Is.EqualTo(1), "viewerA has 1 friend");

        var friend = root[0];
        Assert.That(GetStringifiedLong(friend, "viewer_id"), Is.EqualTo(viewerB));

        // is_join_guild must be present — viewerB has no guild, so false.
        Assert.That(friend.TryGetProperty("is_join_guild", out var isJoin), Is.True,
            "is_join_guild must be present in friend_list entries");
        Assert.That(isJoin.GetBoolean(), Is.False, "viewerB is not in a guild so is_join_guild must be false");
    }

    [Test]
    public async Task FriendList_marks_friend_in_guild_as_is_join_guild_true()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(76_561_198_500_000_007UL, "CrfViewerC");
        long viewerB = await factory.SeedViewerAsync(76_561_198_500_000_008UL, "CrfViewerD");

        // viewerB creates a guild.
        using var clientB = factory.CreateAuthenticatedClient(viewerB);
        await clientB.PostAsync("/guild/create",
            JsonContent.Create(new
            {
                guild_name = "CrfGuildB",
                activity = 1,
                join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk
            }));

        // Make viewerA ↔ viewerB friends.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerA, FriendViewerId = viewerB, CreatedAt = DateTime.UtcNow });
            db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerB, FriendViewerId = viewerA, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        using var clientA = factory.CreateAuthenticatedClient(viewerA);
        var resp = await clientA.PostAsync("/guild/friend_list", JsonContent.Create(BaseReq()));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True);

        using var doc = JsonDocument.Parse(body);
        // Response is a bare array — GuildFriendListTask reads data[i] directly.
        var arr = doc.RootElement;
        Assert.That(arr.ValueKind, Is.EqualTo(JsonValueKind.Array),
            $"friend_list data must be a bare array at root: {body}");
        Assert.That(arr.GetArrayLength(), Is.EqualTo(1));

        var friend = arr[0];
        Assert.That(friend.GetProperty("is_join_guild").GetBoolean(), Is.True,
            "viewerB is in a guild so is_join_guild must be true");
    }

    [Test]
    public async Task FriendList_empty_for_viewer_with_no_friends()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(76_561_198_500_000_009UL, "CrfLoneViewer");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/guild/friend_list", JsonContent.Create(BaseReq()));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True);

        using var doc = JsonDocument.Parse(body);
        // Response is a bare array at root — no wrapper object.
        var root = doc.RootElement;
        Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Array),
            $"friend_list must return a bare array even when empty: {body}");
        Assert.That(root.GetArrayLength(), Is.EqualTo(0), "No friends means empty array");
    }
}
