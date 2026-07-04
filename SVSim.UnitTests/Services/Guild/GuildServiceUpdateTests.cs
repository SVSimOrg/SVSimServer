using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Services.Guild;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Guild;

// ---------------------------------------------------------------------------
// Spy IGuildChatService — counts EmitSystemEventAsync calls per GuildChatMessageType.
// ---------------------------------------------------------------------------
internal sealed class SpyGuildChatService : IGuildChatService
{
    public readonly List<(int GuildId, long ActorId, GuildChatMessageType Type)> Emissions = new();
    public readonly List<(int GuildId, long ActorId, GuildChatMessageType Type, string? Body)> EmissionsWithBody = new();

    public Task EmitSystemEventAsync(int guildId, long actorViewerId, GuildChatMessageType type, string? body = null, CancellationToken ct = default)
    {
        Emissions.Add((guildId, actorViewerId, type));
        EmissionsWithBody.Add((guildId, actorViewerId, type, body));
        return Task.CompletedTask;
    }

    // ---- stub out the rest -----------------------------------------------
    public Task<ChatWindow> GetWindowAsync(long v, int s, int d, int w, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<ChatPostResult> PostTextOrStampAsync(long v, int t, string m, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<ChatPostResult> PostDeckAsync(long v, SVSim.Database.Enums.Format f, int fa, int dn, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<(bool Ok, DeckLogResult? Log)> DeleteDeckAsync(long v, int id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<ChatPostResult> PostReplayAsync(long v, long battleId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<string?> GetReplayDetailAsync(long v, int id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<DeckLogResult?> GetDeckLogAsync(long v, CancellationToken ct = default) => throw new NotImplementedException();
}

// ---------------------------------------------------------------------------
// Factory variant that replaces IGuildChatService with our spy singleton.
// ---------------------------------------------------------------------------
internal sealed class SpyGuildChatFactory : SVSimTestFactory
{
    public readonly SpyGuildChatService ChatSpy = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            // Remove all existing registrations for IGuildChatService.
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IGuildChatService))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            // Register our singleton spy so every scope gets the same instance.
            services.AddSingleton<IGuildChatService>(ChatSpy);
        });
    }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

public class GuildServiceUpdateTests
{
    // -----------------------------------------------------------------------
    // UpdateAsync — activity + join_condition
    // -----------------------------------------------------------------------

    [Test]
    public async Task UpdateAsync_leader_can_change_activity_and_join_condition()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_001UL, "UpdateLeader1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("UpdateGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateAsync(leaderId, new((int)GuildActivity.Rotation, (int)GuildJoinCondition.Approval));
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var g = await db.Guilds.FirstAsync(x => x.GuildId == guildId);
            Assert.That((int)g.Activity, Is.EqualTo((int)GuildActivity.Rotation));
            Assert.That((int)g.JoinCondition, Is.EqualTo((int)GuildJoinCondition.Approval));
        }
    }

    [Test]
    public async Task UpdateAsync_non_leader_returns_PermissionDenied()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_002UL, "UpdateLeader2");
        var memberId = await factory.SeedViewerAsync(76_561_198_200_000_003UL, "UpdateMember2");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("UpdateGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        // Add a regular member.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.GuildMembers.Add(new GuildMember { GuildId = guildId, ViewerId = memberId, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });
            var v = await db.Viewers.FirstAsync(x => x.Id == memberId);
            v.GuildId = guildId;
            await db.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateAsync(memberId, new((int)GuildActivity.Stoic, null));
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied));
        }
    }

    [Test]
    public async Task UpdateAsync_invalid_activity_returns_NameInvalid()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_004UL, "UpdateLeader3");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("UpdateGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            // Activity 0 is out of range (valid: 1..16).
            var r = await svc.UpdateAsync(leaderId, new(0, null));
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.NameInvalid));
        }
    }

    [Test]
    public async Task UpdateAsync_invalid_join_condition_returns_NameInvalid()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_005UL, "UpdateLeader4");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("UpdateGuild4", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            // JoinCondition 99 is out of range (valid: 1..3).
            var r = await svc.UpdateAsync(leaderId, new(null, 99));
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.NameInvalid));
        }
    }

    // -----------------------------------------------------------------------
    // UpdateDescriptionAsync
    // -----------------------------------------------------------------------

    [Test]
    public async Task UpdateDescriptionAsync_leader_updates_description_and_emits_chat_event()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_010UL, "DescLeader1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("DescGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        // Drain any emissions from CreateAsync (CreateGuild event).
        factory.ChatSpy.Emissions.Clear();

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateDescriptionAsync(leaderId, "New description text");
            Assert.That(r.IsOk, Is.True);
        }

        // Verify DB updated.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var g = await db.Guilds.FirstAsync(x => x.GuildId == guildId);
            Assert.That(g.Description, Is.EqualTo("New description text"));
        }

        // Verify exactly one Description chat event was emitted.
        Assert.That(factory.ChatSpy.Emissions.Count, Is.EqualTo(1));
        Assert.That(factory.ChatSpy.Emissions[0].Type, Is.EqualTo(GuildChatMessageType.Description));
        Assert.That(factory.ChatSpy.Emissions[0].GuildId, Is.EqualTo(guildId));
        Assert.That(factory.ChatSpy.Emissions[0].ActorId, Is.EqualTo(leaderId));
    }

    [Test]
    public async Task UpdateDescriptionAsync_non_leader_returns_PermissionDenied()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_011UL, "DescLeader2");
        var memberId = await factory.SeedViewerAsync(76_561_198_200_000_012UL, "DescMember2");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("DescGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.GuildMembers.Add(new GuildMember { GuildId = guildId, ViewerId = memberId, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });
            var v = await db.Viewers.FirstAsync(x => x.Id == memberId);
            v.GuildId = guildId;
            await db.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateDescriptionAsync(memberId, "Hacked");
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied));
        }
    }

    [Test]
    public async Task UpdateDescriptionAsync_over_512_chars_returns_NameInvalid()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_013UL, "DescLeader3");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("DescGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var tooLong = new string('x', 513);
            var r = await svc.UpdateDescriptionAsync(leaderId, tooLong);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.NameInvalid));
        }
    }

    // -----------------------------------------------------------------------
    // UpdateEmblemAsync
    // -----------------------------------------------------------------------

    [Test]
    public async Task UpdateEmblemAsync_leader_updates_emblem_id()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_020UL, "EmblemLeader1");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("EmblemGuild1", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        const long newEmblemId = 100_000_042L;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateEmblemAsync(leaderId, newEmblemId);
            Assert.That(r.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var g = await db.Guilds.FirstAsync(x => x.GuildId == guildId);
            Assert.That(g.EmblemId, Is.EqualTo(newEmblemId));
        }
    }

    [Test]
    public async Task UpdateEmblemAsync_non_leader_returns_PermissionDenied()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_021UL, "EmblemLeader2");
        var memberId = await factory.SeedViewerAsync(76_561_198_200_000_022UL, "EmblemMember2");

        int guildId;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var res = await svc.CreateAsync(leaderId, new("EmblemGuild2", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(res.IsOk, Is.True);
            guildId = res.GuildId!.Value;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.GuildMembers.Add(new GuildMember { GuildId = guildId, ViewerId = memberId, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });
            var v = await db.Viewers.FirstAsync(x => x.Id == memberId);
            v.GuildId = guildId;
            await db.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateEmblemAsync(memberId, 999L);
            Assert.That(r.Code, Is.EqualTo(GuildOpResultCode.PermissionDenied));
        }
    }

    [Test]
    public async Task UpdateEmblemAsync_does_NOT_emit_chat_event()
    {
        using var factory = new SpyGuildChatFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_200_000_023UL, "EmblemLeader3");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            await svc.CreateAsync(leaderId, new("EmblemGuild3", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        // Drain CreateGuild system event.
        factory.ChatSpy.Emissions.Clear();

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var r = await svc.UpdateEmblemAsync(leaderId, 100_000_099L);
            Assert.That(r.IsOk, Is.True);
        }

        Assert.That(factory.ChatSpy.Emissions.Count, Is.EqualTo(0),
            "UpdateEmblemAsync must NOT emit a chat system event");
    }
}
