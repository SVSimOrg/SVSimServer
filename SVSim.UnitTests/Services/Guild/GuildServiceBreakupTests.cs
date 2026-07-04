using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Services.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Guild;

public class GuildServiceBreakupTests
{
    private const string BaseReq = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    [Test]
    public async Task BreakupAsync_soft_deletes_guild_and_cascade_hard_deletes_dependents()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_100_000_010UL, "BreakupLeader");
        var memberId = await factory.SeedViewerAsync(76_561_198_100_000_011UL, "BreakupMember");

        int guildId;

        // Create the guild as leader.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("BreakupGuild", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        // Add member2 directly via DbContext (simulates a later join).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.GuildMembers.Add(new GuildMember
            {
                GuildId = guildId,
                ViewerId = memberId,
                Role = GuildRole.Regular,
                JoinedAt = DateTime.UtcNow,
            });
            var viewer = await db.Viewers.FirstAsync(v => v.Id == memberId);
            viewer.GuildId = guildId;
            await db.SaveChangesAsync();
        }

        // Breakup as leader.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.BreakupAsync(leaderId);
            Assert.That(res.IsOk, Is.True);
        }

        // Assert: guild has BreakupAt set.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId);
            Assert.That(guild, Is.Not.Null, "Guild row should still exist (soft-delete)");
            Assert.That(guild!.BreakupAt, Is.Not.Null, "BreakupAt must be set");

            // 0 members remain.
            var memberCount = await db.GuildMembers.CountAsync(m => m.GuildId == guildId);
            Assert.That(memberCount, Is.EqualTo(0), "All GuildMember rows must be hard-deleted");

            // Both viewers' GuildId is null.
            var leaderViewer = await db.Viewers.FirstAsync(v => v.Id == leaderId);
            var memberViewer = await db.Viewers.FirstAsync(v => v.Id == memberId);
            Assert.That(leaderViewer.GuildId, Is.Null, "Leader's GuildId should be cleared");
            Assert.That(memberViewer.GuildId, Is.Null, "Member's GuildId should be cleared");
        }
    }

    [Test]
    public async Task BreakupAsync_returns_PermissionDenied_for_non_leader()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_100_000_012UL, "NLLeader");
        var memberId = await factory.SeedViewerAsync(76_561_198_100_000_013UL, "NLMember");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("NLGuild", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        // Directly add member.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.GuildMembers.Add(new GuildMember
            {
                GuildId = guildId,
                ViewerId = memberId,
                Role = GuildRole.Regular,
                JoinedAt = DateTime.UtcNow,
            });
            var viewer = await db.Viewers.FirstAsync(v => v.Id == memberId);
            viewer.GuildId = guildId;
            await db.SaveChangesAsync();
        }

        // Non-leader tries to break up.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.BreakupAsync(memberId);
            Assert.That(res.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied));
        }
    }

    [Test]
    public async Task GuildInfo_JOINING_response_populates_leader_name()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync(76_561_198_100_000_014UL, "LeaderDisplayName");

        // Create guild.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(viewerId, new("LeaderNameGuild", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
        }

        // Hit /guild/info via HTTP (middleware is no-op in test mode — no UnityPlayer UA).
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/guild/info",
            new StringContent(BaseReq, Encoding.UTF8, "application/json"));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.IsSuccessStatusCode, Is.True, $"guild/info failed: {body}");

        using var doc = JsonDocument.Parse(body);
        var leaderName = doc.RootElement
            .GetProperty("guild")
            .GetProperty("detail")
            .GetProperty("leader_name")
            .GetString();

        Assert.That(leaderName, Is.EqualTo("LeaderDisplayName"),
            "leader_name in guild/info detail must equal the viewer's display name");
    }
}
