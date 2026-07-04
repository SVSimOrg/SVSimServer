using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Services.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Guild;

public class GuildServiceChangeRoleTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helper: add a member row + set Viewer.GuildId
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
    // ChangeRoleAsync tests
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task ChangeRoleAsync_leader_promotes_regular_to_subleader_succeeds()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_001UL, "CrLeader1");
        var memberId = await factory.SeedViewerAsync(76_561_198_300_000_002UL, "CrMember1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, memberId, (int)GuildRole.SubLeader);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var m = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == memberId);
            Assert.That(m, Is.Not.Null);
            Assert.That(m!.Role, Is.EqualTo(GuildRole.SubLeader), "Member must be promoted to SubLeader");

            // Leader's role must be unchanged.
            var leader = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == leaderId);
            Assert.That(leader!.Role, Is.EqualTo(GuildRole.Leader), "Leader role must be unchanged after promotion");
        }
    }

    [Test]
    public async Task ChangeRoleAsync_promote_blocked_when_subleader_cap_reached()
    {
        using var factory = new SVSimTestFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_300_000_003UL, "CrLeader2");
        var sub1Id    = await factory.SeedViewerAsync(76_561_198_300_000_004UL, "CrSub1");
        var sub2Id    = await factory.SeedViewerAsync(76_561_198_300_000_005UL, "CrSub2");
        var regularId = await factory.SeedViewerAsync(76_561_198_300_000_006UL, "CrReg1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        // Seed 2 existing subleaders (MaxSubLeaderNum = 2 by default).
        await AddMemberDirectlyAsync(factory, guildId, sub1Id, GuildRole.SubLeader);
        await AddMemberDirectlyAsync(factory, guildId, sub2Id, GuildRole.SubLeader);
        await AddMemberDirectlyAsync(factory, guildId, regularId, GuildRole.Regular);

        // Attempt to promote regularId to SubLeader — should fail.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, regularId, (int)GuildRole.SubLeader);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.SubLeaderCapReached),
                "Promoting to SubLeader beyond cap must be blocked");
        }

        // regularId's role must still be Regular.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var m = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == regularId);
            Assert.That(m!.Role, Is.EqualTo(GuildRole.Regular), "Role must not change on cap-blocked promote");
        }
    }

    [Test]
    public async Task ChangeRoleAsync_leader_demotes_subleader_to_regular_succeeds()
    {
        using var factory = new SVSimTestFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_300_000_007UL, "CrLeader3");
        var subId     = await factory.SeedViewerAsync(76_561_198_300_000_008UL, "CrSub2");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subId, GuildRole.SubLeader);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, subId, (int)GuildRole.Regular);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var m = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == subId);
            Assert.That(m!.Role, Is.EqualTo(GuildRole.Regular), "SubLeader must be demoted to Regular");
        }
    }

    [Test]
    public async Task ChangeRoleAsync_atomic_leader_transfer_sets_target_leader_and_caller_regular()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_300_000_009UL, "CrLeader4");
        var targetId  = await factory.SeedViewerAsync(76_561_198_300_000_010UL, "CrTarget4");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild4", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, targetId, GuildRole.Regular);

        // Leader transfers leadership to targetId.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, targetId, (int)GuildRole.Leader);
            Assert.That(r.IsOk, Is.True);
        }

        // Verify roles + guild.LeaderViewerId atomically committed.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            var targetMember = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == targetId);
            Assert.That(targetMember!.Role, Is.EqualTo(GuildRole.Leader), "Target must become Leader");

            var callerMember = await db.GuildMembers.FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == leaderId);
            Assert.That(callerMember!.Role, Is.EqualTo(GuildRole.Regular), "Caller must become Regular after transfer");

            var guild = await db.Guilds.FirstAsync(g => g.GuildId == guildId);
            Assert.That(guild.LeaderViewerId, Is.EqualTo(targetId), "Guild.LeaderViewerId must point to new leader");
        }

        // ChangeLeader (6) event must have been emitted.
        var leaderEmissions = factory.ChatSpy.EmissionsWithBody
            .Where(e => e.Type == GuildChatMessageType.ChangeLeader && e.GuildId == guildId)
            .ToList();
        Assert.That(leaderEmissions, Has.Count.GreaterThanOrEqualTo(1), "ChangeLeader event must be emitted");
        Assert.That(leaderEmissions[0].Body, Does.Contain(targetId.ToString()),
            "ChangeLeader body must contain new leader's viewer_id");
    }

    [Test]
    public async Task ChangeRoleAsync_atomic_leader_transfer_from_subleader_target()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId  = await factory.SeedViewerAsync(76_561_198_300_000_011UL, "CrLeader5");
        var subId     = await factory.SeedViewerAsync(76_561_198_300_000_012UL, "CrSub5");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild5", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subId, GuildRole.SubLeader);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, subId, (int)GuildRole.Leader);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var target = await db.GuildMembers.FirstAsync(m => m.GuildId == guildId && m.ViewerId == subId);
            Assert.That(target.Role, Is.EqualTo(GuildRole.Leader));
            var caller = await db.GuildMembers.FirstAsync(m => m.GuildId == guildId && m.ViewerId == leaderId);
            Assert.That(caller.Role, Is.EqualTo(GuildRole.Regular));
            var guild = await db.Guilds.FirstAsync(g => g.GuildId == guildId);
            Assert.That(guild.LeaderViewerId, Is.EqualTo(subId));
        }
    }

    [Test]
    public async Task ChangeRoleAsync_non_leader_caller_rejected()
    {
        using var factory = new SVSimTestFactory();
        var leaderId   = await factory.SeedViewerAsync(76_561_198_300_000_013UL, "CrLeader6");
        var subId      = await factory.SeedViewerAsync(76_561_198_300_000_014UL, "CrSub6");
        var regularId  = await factory.SeedViewerAsync(76_561_198_300_000_015UL, "CrReg6");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild6", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subId, GuildRole.SubLeader);
        await AddMemberDirectlyAsync(factory, guildId, regularId, GuildRole.Regular);

        // SubLeader tries to change someone's role — must be rejected.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(subId, regularId, (int)GuildRole.SubLeader);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied),
                "Non-leader must not be able to change roles");
        }
    }

    [Test]
    public async Task ChangeRoleAsync_same_role_is_noop_returns_ok()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_016UL, "CrLeader7");
        var subId    = await factory.SeedViewerAsync(76_561_198_300_000_017UL, "CrSub7");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild7", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, subId, GuildRole.SubLeader);

        // Setting same role (SubLeader → SubLeader) — must succeed as no-op.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, subId, (int)GuildRole.SubLeader);
            Assert.That(r.IsOk, Is.True, "Same-role set must return Ok (no-op)");
        }

        // Role must be unchanged.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var m = await db.GuildMembers.FirstAsync(m => m.GuildId == guildId && m.ViewerId == subId);
            Assert.That(m.Role, Is.EqualTo(GuildRole.SubLeader), "No-op must leave role unchanged");
        }
    }

    [Test]
    public async Task ChangeRoleAsync_promotes_subleader_and_emits_change_subleader_event()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_018UL, "CrLeader8");
        var memberId = await factory.SeedViewerAsync(76_561_198_300_000_019UL, "CrMember8");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.CreateAsync(leaderId, new("CrGuild8", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(r.IsOk, Is.True);
            guildId = r.GuildId!.Value;
        }

        await AddMemberDirectlyAsync(factory, guildId, memberId, GuildRole.Regular);

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.ChangeRoleAsync(leaderId, memberId, (int)GuildRole.SubLeader);
            Assert.That(r.IsOk, Is.True);
        }

        // ChangeSubLeader (7) event must be emitted.
        var subLeaderEmissions = factory.ChatSpy.EmissionsWithBody
            .Where(e => e.Type == GuildChatMessageType.ChangeSubLeader && e.GuildId == guildId)
            .ToList();
        Assert.That(subLeaderEmissions, Has.Count.GreaterThanOrEqualTo(1), "ChangeSubLeader event must be emitted on promotion");
        Assert.That(subLeaderEmissions[0].Body, Does.Contain(memberId.ToString()),
            "ChangeSubLeader body must contain target's viewer_id");
    }
}
