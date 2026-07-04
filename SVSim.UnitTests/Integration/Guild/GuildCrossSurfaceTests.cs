using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.UnitTests.Infrastructure;
using GuildEntity = SVSim.Database.Entities.Guild.Guild;

namespace SVSim.UnitTests.Integration.Guild;

/// <summary>
/// Cross-surface integration tests for guild state flowing into /mypage/index
/// guild_notification. Verifies that the four GuildNotification fields are populated
/// correctly (or left null/false) based on real DB state.
/// </summary>
public class GuildCrossSurfaceTests
{
    private const string MyPageRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam"}""";

    private static StringContent JsonBody(string json)
        => new(json, Encoding.UTF8, "application/json");

    /// <summary>Seeds a guild + makes the viewer a Leader member. Returns the guild's GuildId.</summary>
    private static async Task<int> SeedGuildWithMemberAsync(SVSimTestFactory factory, long viewerId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var guild = new GuildEntity
        {
            GuildId = 100_000_001,
            Name = "TestGuild",
            Description = "Test guild for cross-surface tests",
            LeaderViewerId = viewerId,
            EmblemId = 1,
            Activity = GuildActivity.All,
            JoinCondition = GuildJoinCondition.Free,
            CreatedAt = DateTime.UtcNow,
        };
        guild.Members.Add(new GuildMember
        {
            GuildId = guild.GuildId,
            ViewerId = viewerId,
            Role = GuildRole.Leader,
            JoinedAt = DateTime.UtcNow,
        });
        db.Guilds.Add(guild);
        await db.SaveChangesAsync();
        return guild.GuildId;
    }

    [Test]
    public async Task Viewer_with_guild_gets_guild_id_populated()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int guildId = await SeedGuildWithMemberAsync(factory, viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mypage/index", JsonBody(MyPageRequestJson));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        var notification = root.GetProperty("guild_notification");

        Assert.That(notification.GetProperty("guild_id").GetInt32(), Is.EqualTo(guildId),
            "guild_id should be the viewer's guild id");
        Assert.That(notification.GetProperty("is_invited").GetBoolean(), Is.False);
        Assert.That(notification.GetProperty("is_join_request").GetBoolean(), Is.False);
    }

    [Test]
    public async Task Viewer_with_pending_invite_gets_is_invited_true()
    {
        using var factory = new SVSimTestFactory();
        long inviterViewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL, displayName: "Inviter");
        long inviteeViewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_003UL, displayName: "Invitee");

        // Seed a guild for the inviter
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var guild = new GuildEntity
            {
                GuildId = 100_000_002,
                Name = "InviterGuild",
                Description = "",
                LeaderViewerId = inviterViewerId,
                EmblemId = 1,
                Activity = GuildActivity.All,
                JoinCondition = GuildJoinCondition.Free,
                CreatedAt = DateTime.UtcNow,
            };
            guild.Members.Add(new GuildMember
            {
                GuildId = guild.GuildId,
                ViewerId = inviterViewerId,
                Role = GuildRole.Leader,
                JoinedAt = DateTime.UtcNow,
            });
            db.Guilds.Add(guild);
            // Seed a pending invite for the invitee
            db.GuildInvites.Add(new GuildInvite
            {
                GuildId = guild.GuildId,
                InviterViewerId = inviterViewerId,
                InviteeViewerId = inviteeViewerId,
                Status = GuildInviteStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(inviteeViewerId);
        var resp = await client.PostAsync("/mypage/index", JsonBody(MyPageRequestJson));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        var notification = root.GetProperty("guild_notification");

        Assert.That(notification.GetProperty("is_invited").GetBoolean(), Is.True,
            "is_invited should be true when viewer has a pending invite");
        Assert.That(notification.GetProperty("guild_id").ValueKind, Is.EqualTo(JsonValueKind.Null),
            "guild_id should be null — invitee has no guild yet");
    }

    [Test]
    public async Task Viewer_with_pending_join_request_gets_is_join_request_true()
    {
        using var factory = new SVSimTestFactory();
        long applicantViewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_004UL, displayName: "Applicant");
        long leaderViewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_005UL, displayName: "Leader");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var guild = new GuildEntity
            {
                GuildId = 100_000_003,
                Name = "RequestTargetGuild",
                Description = "",
                LeaderViewerId = leaderViewerId,
                EmblemId = 1,
                Activity = GuildActivity.All,
                JoinCondition = GuildJoinCondition.Approval,
                CreatedAt = DateTime.UtcNow,
            };
            guild.Members.Add(new GuildMember
            {
                GuildId = guild.GuildId,
                ViewerId = leaderViewerId,
                Role = GuildRole.Leader,
                JoinedAt = DateTime.UtcNow,
            });
            db.Guilds.Add(guild);
            db.GuildJoinRequests.Add(new GuildJoinRequest
            {
                GuildId = guild.GuildId,
                ViewerId = applicantViewerId,
                Status = GuildJoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(applicantViewerId);
        var resp = await client.PostAsync("/mypage/index", JsonBody(MyPageRequestJson));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        var notification = root.GetProperty("guild_notification");

        Assert.That(notification.GetProperty("is_join_request").GetBoolean(), Is.True,
            "is_join_request should be true when viewer has a pending join request");
        Assert.That(notification.GetProperty("guild_id").ValueKind, Is.EqualTo(JsonValueKind.Null),
            "guild_id should be null — applicant is not yet a member");
    }

    [Test]
    public async Task Viewer_with_no_guild_and_no_pending_gets_all_null_shape()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_006UL, displayName: "NoGuild");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mypage/index", JsonBody(MyPageRequestJson));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        var notification = root.GetProperty("guild_notification");

        // Fields with [JsonIgnore(Condition = Never)] serialize as explicit null/false.
        Assert.That(notification.GetProperty("guild_id").ValueKind, Is.EqualTo(JsonValueKind.Null),
            "guild_id must be explicit null (not absent) for viewers with no guild");
        Assert.That(notification.GetProperty("guild_room_message_id").ValueKind, Is.EqualTo(JsonValueKind.Null),
            "guild_room_message_id must be explicit null for viewers with no guild");
        Assert.That(notification.GetProperty("is_invited").GetBoolean(), Is.False);
        Assert.That(notification.GetProperty("is_join_request").GetBoolean(), Is.False);
    }
}
