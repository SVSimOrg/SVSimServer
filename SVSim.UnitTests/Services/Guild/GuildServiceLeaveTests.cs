using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Services.Guild;
using SVSim.UnitTests.Infrastructure;
using GuildEntity = SVSim.Database.Entities.Guild.Guild;

namespace SVSim.UnitTests.Services.Guild;

public class GuildServiceLeaveTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helper: add a member row + set Viewer.GuildId in one scope
    // ──────────────────────────────────────────────────────────────────────────
    private static async Task AddMemberDirectlyAsync(
        SVSimTestFactory factory,
        int guildId,
        long viewerId,
        GuildRole role = GuildRole.Regular)
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

    // ──────────────────────────────────────────────────────────────────────────
    // LeaveAsync tests
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task LeaveAsync_regular_member_leaves_guild_persists_and_chat_event_fires()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_200_000_001UL, "LvLeader");
        var memberId  = await factory.SeedViewerAsync(76_561_198_200_000_002UL, "LvMember");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("LeaveGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        // Regular member leaves.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.LeaveAsync(memberId);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            // Guild must still exist.
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId);
            Assert.That(guild, Is.Not.Null, "Guild should persist after regular member leaves");
            Assert.That(guild!.BreakupAt, Is.Null, "Guild must not be soft-deleted");

            // Member row must be gone.
            var memberRow = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == memberId);
            Assert.That(memberRow, Is.Null, "GuildMember row must be deleted");

            // Viewer.GuildId must be cleared.
            var viewer = await db.Viewers.FirstAsync(v => v.Id == memberId);
            Assert.That(viewer.GuildId, Is.Null, "Viewer.GuildId must be null after leave");
        }

        // Chat event: Leave emitted via spy.
        var leaveEmissions = factory.ChatSpy.Emissions
            .Where(e => e.Type == GuildChatMessageType.Leave && e.GuildId == guildId && e.ActorId == memberId)
            .ToList();
        Assert.That(leaveEmissions, Has.Count.GreaterThanOrEqualTo(1), "Leave chat event must be emitted");
    }

    [Test]
    public async Task LeaveAsync_subleader_leaves_same_as_regular_member()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId     = await factory.SeedViewerAsync(76_561_198_200_000_003UL, "LvLeader2");
        var subLeaderId  = await factory.SeedViewerAsync(76_561_198_200_000_004UL, "LvSubLeader");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("LeaveGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subLeaderId, GuildRole.SubLeader);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.LeaveAsync(subLeaderId);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var memberRow = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == subLeaderId);
            Assert.That(memberRow, Is.Null, "SubLeader member row must be deleted after leave");
            var viewer = await db.Viewers.FirstAsync(v => v.Id == subLeaderId);
            Assert.That(viewer.GuildId, Is.Null, "SubLeader.GuildId must be null after leave");
        }

        // Chat spy: Leave event emitted.
        var leaveEmissions = factory.ChatSpy.Emissions
            .Where(e => e.Type == GuildChatMessageType.Leave && e.GuildId == guildId && e.ActorId == subLeaderId)
            .ToList();
        Assert.That(leaveEmissions, Has.Count.GreaterThanOrEqualTo(1), "SubLeader leave must emit Leave chat event");
    }

    [Test]
    public async Task LeaveAsync_leader_with_remaining_members_is_blocked()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_005UL, "LvLeader3");
        var memberId = await factory.SeedViewerAsync(76_561_198_200_000_006UL, "LvMember3");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("LeaveGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.LeaveAsync(leaderId);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.LeaderLeaveBlocked),
                "Leader with remaining members must be blocked from leaving");
        }
    }

    [Test]
    public async Task LeaveAsync_sole_member_leader_auto_routes_to_breakup()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_007UL, "LvLeader4");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("LeaveGuild4", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        // Sole-member leader leaves.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.LeaveAsync(leaderId);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            // Guild should be soft-deleted (BreakupAt set).
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId);
            Assert.That(guild, Is.Not.Null);
            Assert.That(guild!.BreakupAt, Is.Not.Null, "Sole-member leader leave must trigger breakup (BreakupAt set)");

            // No member rows should remain.
            var count = await db.GuildMembers.CountAsync(m => m.GuildId == guildId);
            Assert.That(count, Is.EqualTo(0), "All member rows should be deleted via breakup");

            // Viewer.GuildId cleared.
            var viewer = await db.Viewers.FirstAsync(v => v.Id == leaderId);
            Assert.That(viewer.GuildId, Is.Null, "Leader.GuildId must be null after sole-member breakup");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RemoveAsync tests
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task RemoveAsync_leader_removes_regular_member_row_deleted_chat_event_fires()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_010UL, "RmLeader1");
        var memberId = await factory.SeedViewerAsync(76_561_198_200_000_011UL, "RmMember1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("RemoveGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.RemoveAsync(leaderId, memberId);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            // Target member row gone.
            var memberRow = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == memberId);
            Assert.That(memberRow, Is.Null, "Removed member row must be deleted");

            // Target Viewer.GuildId cleared.
            var viewer = await db.Viewers.FirstAsync(v => v.Id == memberId);
            Assert.That(viewer.GuildId, Is.Null, "Removed member's GuildId must be null");

            // Leader row still present.
            var leaderRow = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == leaderId);
            Assert.That(leaderRow, Is.Not.Null, "Leader must still be in the guild");
        }

        // Remove chat event: type=Remove, actor=leader, body contains target viewer id.
        var removeEmissions = factory.ChatSpy.EmissionsWithBody
            .Where(e => e.Type == GuildChatMessageType.Remove && e.GuildId == guildId && e.ActorId == leaderId)
            .ToList();
        Assert.That(removeEmissions, Has.Count.GreaterThanOrEqualTo(1), "Remove chat event must be emitted");
        Assert.That(removeEmissions[0].Body, Does.Contain(memberId.ToString()),
            "Remove event body must contain the target's viewer_id");
    }

    [Test]
    public async Task RemoveAsync_non_leader_is_rejected()
    {
        using var factory = new SVSimTestFactory();
        var leaderId    = await factory.SeedViewerAsync(76_561_198_200_000_012UL, "RmLeader2");
        var subLeaderId = await factory.SeedViewerAsync(76_561_198_200_000_013UL, "RmSubLdr2");
        var targetId    = await factory.SeedViewerAsync(76_561_198_200_000_014UL, "RmTarget2");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.CreateAsync(leaderId, new("RemoveGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subLeaderId, GuildRole.SubLeader);
        await AddMemberDirectlyAsync(factory, guildId, targetId, GuildRole.Regular);

        // SubLeader attempts to kick — must be rejected (only Leader can kick).
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.RemoveAsync(subLeaderId, targetId);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied),
                "SubLeader must not be allowed to remove members");
        }

        // Target must still be in the guild.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var memberRow = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == targetId);
            Assert.That(memberRow, Is.Not.Null, "Target must still be in the guild after failed remove");
        }
    }

    [Test]
    public async Task RemoveAsync_leader_cannot_remove_themselves()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_015UL, "RmLeader3");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("RemoveGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.RemoveAsync(leaderId, leaderId);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied),
                "Leader must not be able to remove themselves");
        }
    }

    [Test]
    public async Task RemoveAsync_target_not_in_callers_guild_is_rejected()
    {
        using var factory = new SVSimTestFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_200_000_016UL, "RmLeader4");
        var outsiderId = await factory.SeedViewerAsync(76_561_198_200_000_017UL, "Outsider4");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("RemoveGuild4", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        // outsiderId is not in any guild.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r   = await svc.RemoveAsync(leaderId, outsiderId);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.TargetNotInGuild),
                "Target not in caller's guild must be rejected");
        }
    }
}
