using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Services.Friend;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class FriendServiceTests
{
    private static async Task<long> SeedViewer(SVSimTestFactory factory, ulong steamId, string name = "Test Viewer")
        => await factory.SeedViewerAsync(steamId: steamId, displayName: name);

    private static IFriendService Service(SVSimTestFactory factory, out IServiceScope scope)
    {
        scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IFriendService>();
    }

    private static SVSimDbContext Ctx(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

    [Test]
    public async Task GetFriendsAsync_returns_empty_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_000_001UL);

        var svc = Service(factory, out var scope);
        using (scope)
        {
            var result = await svc.GetFriendsAsync(viewerId, default);
            Assert.That(result.Friends, Is.Empty);
            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(result.MaxCount, Is.EqualTo(110));
        }
    }

    [Test]
    public async Task GetFriendsAsync_returns_15_field_entries_for_seeded_friend()
    {
        using var factory = new SVSimTestFactory();
        long owner = await SeedViewer(factory, 76_561_198_000_000_002UL, "Owner");
        long friend = await SeedViewer(factory, 76_561_198_000_000_003UL, "Friend");

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = owner, FriendViewerId = friend, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetFriendsAsync(owner, default);
            Assert.That(result.Friends, Has.Count.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(1));
            var entry = result.Friends[0];
            Assert.That(entry.ViewerId, Is.EqualTo((int)friend));
            Assert.That(entry.Name, Is.EqualTo("Friend"));
            Assert.That(entry.DeviceType, Is.EqualTo("2"));
            Assert.That(entry.MaxFriend, Is.EqualTo("110"));
            Assert.That(entry.IsOfficial, Is.EqualTo("0"));
        }
    }

    [Test]
    public async Task GetReceiveAppliesAsync_returns_incoming_with_correct_viewer_id_and_id()
    {
        using var factory = new SVSimTestFactory();
        long target = await SeedViewer(factory, 76_561_198_000_000_004UL, "Target");
        long sender = await SeedViewer(factory, 76_561_198_000_000_005UL, "Sender");

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetReceiveAppliesAsync(target, default);
            Assert.That(result.ReceiveApplies, Has.Count.EqualTo(1));
            Assert.That(result.ReceiveApplies[0].Id, Is.EqualTo(applyId));
            Assert.That(result.ReceiveApplies[0].ViewerId, Is.EqualTo((int)sender), "viewer_id is the SENDER's id");
            Assert.That(result.ReceiveApplies[0].Name, Is.EqualTo("Sender"));
        }
    }

    [Test]
    public async Task GetSendAppliesAsync_returns_outgoing_with_remaining_count()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_000_006UL, "Sender");
        long target = await SeedViewer(factory, 76_561_198_000_000_007UL, "Target");

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetSendAppliesAsync(sender, default);
            Assert.That(result.SendApplies, Has.Count.EqualTo(1));
            Assert.That(result.SendApplyMaxCount, Is.EqualTo(110));
            Assert.That(result.RemainingApplyCount, Is.EqualTo(109));
            Assert.That(result.SendApplies[0].ViewerId, Is.EqualTo((int)target), "viewer_id is the TARGET's id");
        }
    }

    [Test]
    public async Task GetPlayedTogetherAsync_returns_empty_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_000_008UL);

        var svc = Service(factory, out var scope);
        using (scope)
        {
            var result = await svc.GetPlayedTogetherAsync(viewerId, default);
            Assert.That(result.Histories, Is.Empty);
        }
    }

    [Test]
    public async Task GetPlayedTogetherAsync_computes_friend_status_NO_ACTION_for_stranger()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_009UL, "Me");
        long opponent = await SeedViewer(factory, 76_561_198_000_000_010UL, "Opponent");

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = me, OpponentViewerId = opponent,
                PlayedAt = DateTime.UtcNow, PlayedMode = 1, BattleType = 2, DeckFormat = 3, TwoPickType = 4,
            });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetPlayedTogetherAsync(me, default);
            Assert.That(result.Histories, Has.Count.EqualTo(1));
            Assert.That(result.Histories[0].FriendStatus, Is.EqualTo(0), "no apply, no friendship → NO_ACTION");
            Assert.That(result.Histories[0].FriendApplyId, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task GetPlayedTogetherAsync_computes_friend_status_IS_FRIEND_for_friend()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_011UL, "Me");
        long friend = await SeedViewer(factory, 76_561_198_000_000_012UL, "Friend");

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = me, FriendViewerId = friend, CreatedAt = DateTime.UtcNow });
            ctx.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = me, OpponentViewerId = friend,
                PlayedAt = DateTime.UtcNow, PlayedMode = 1, BattleType = 2, DeckFormat = 3, TwoPickType = 4,
            });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetPlayedTogetherAsync(me, default);
            Assert.That(result.Histories[0].FriendStatus, Is.EqualTo(1), "IS_FRIEND");
        }
    }

    [Test]
    public async Task GetPlayedTogetherAsync_computes_friend_status_IS_SEND_with_apply_id()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_013UL, "Me");
        long target = await SeedViewer(factory, 76_561_198_000_000_014UL, "Target");

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = me, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            ctx.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = me, OpponentViewerId = target,
                PlayedAt = DateTime.UtcNow, PlayedMode = 1, BattleType = 2, DeckFormat = 3, TwoPickType = 4,
            });
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetPlayedTogetherAsync(me, default);
            Assert.That(result.Histories[0].FriendStatus, Is.EqualTo(2), "IS_SEND");
            Assert.That(result.Histories[0].FriendApplyId, Is.EqualTo(applyId));
        }
    }

    [Test]
    public async Task GetPlayedTogetherAsync_computes_friend_status_IS_RECEIVED_with_apply_id()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_015UL, "Me");
        long sender = await SeedViewer(factory, 76_561_198_000_000_016UL, "Sender");

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = me, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            ctx.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = me, OpponentViewerId = sender,
                PlayedAt = DateTime.UtcNow, PlayedMode = 1, BattleType = 2, DeckFormat = 3, TwoPickType = 4,
            });
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2)
        {
            var result = await svc.GetPlayedTogetherAsync(me, default);
            Assert.That(result.Histories[0].FriendStatus, Is.EqualTo(3), "IS_RECEIVED");
            Assert.That(result.Histories[0].FriendApplyId, Is.EqualTo(applyId));
        }
    }

    [Test]
    public async Task SearchAsync_returns_entry_for_existing_viewer()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_017UL, "Me");
        long target = await SeedViewer(factory, 76_561_198_000_000_018UL, "Target");

        var svc = Service(factory, out var scope);
        using (scope)
        {
            var result = await svc.SearchAsync(me, (int)target, default);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Target"));
        }
    }

    [Test]
    public async Task SearchAsync_returns_null_for_self_search()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_019UL);

        var svc = Service(factory, out var scope);
        using (scope)
        {
            var result = await svc.SearchAsync(me, (int)me, default);
            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public async Task SearchAsync_returns_null_for_unknown_viewer_id()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_000_020UL);

        var svc = Service(factory, out var scope);
        using (scope)
        {
            var result = await svc.SearchAsync(me, 999_999_999, default);
            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public async Task SendApplyAsync_creates_apply_row()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_001_001UL);
        long target = await SeedViewer(factory, 76_561_198_000_001_002UL);

        var svc = Service(factory, out var scope);
        using (scope) await svc.SendApplyAsync(sender, (int)target, default);

        using var verifyScope = factory.Services.CreateScope();
        var ctx = Ctx(verifyScope);
        var applies = await ctx.ViewerFriendApplies.AsNoTracking().Where(a => a.FromViewerId == sender).ToListAsync();
        Assert.That(applies, Has.Count.EqualTo(1));
        Assert.That(applies[0].ToViewerId, Is.EqualTo(target));
    }

    [Test]
    public async Task SendApplyAsync_no_op_for_self()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_001_003UL);

        var svc = Service(factory, out var scope);
        using (scope) await svc.SendApplyAsync(sender, (int)sender, default);

        using var verifyScope = factory.Services.CreateScope();
        var count = await Ctx(verifyScope).ViewerFriendApplies.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task SendApplyAsync_no_op_for_unknown_target()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_001_004UL);

        var svc = Service(factory, out var scope);
        using (scope) await svc.SendApplyAsync(sender, 999_999_999, default);

        using var verifyScope = factory.Services.CreateScope();
        var count = await Ctx(verifyScope).ViewerFriendApplies.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task SendApplyAsync_no_op_when_already_friends()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_001_005UL);
        long friend = await SeedViewer(factory, 76_561_198_000_001_006UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = me, FriendViewerId = friend, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.SendApplyAsync(me, (int)friend, default);

        using var verifyScope = factory.Services.CreateScope();
        var count = await Ctx(verifyScope).ViewerFriendApplies.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task SendApplyAsync_no_op_when_already_pending()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_001_007UL);
        long target = await SeedViewer(factory, 76_561_198_000_001_008UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.SendApplyAsync(sender, (int)target, default);

        using var verifyScope = factory.Services.CreateScope();
        var count = await Ctx(verifyScope).ViewerFriendApplies.CountAsync();
        Assert.That(count, Is.EqualTo(1), "Pre-existing apply must not be duplicated");
    }

    [Test]
    public async Task ApproveApplyAsync_creates_two_friend_rows_and_deletes_apply()
    {
        using var factory = new SVSimTestFactory();
        long target = await SeedViewer(factory, 76_561_198_000_001_009UL);
        long sender = await SeedViewer(factory, 76_561_198_000_001_010UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.ApproveApplyAsync(target, applyId, default);

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = Ctx(verifyScope);
        var friends = await ctx2.ViewerFriends.AsNoTracking().ToListAsync();
        Assert.That(friends, Has.Count.EqualTo(2));
        Assert.That(friends.Any(f => f.OwnerViewerId == target && f.FriendViewerId == sender));
        Assert.That(friends.Any(f => f.OwnerViewerId == sender && f.FriendViewerId == target));
        Assert.That(await ctx2.ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ApproveApplyAsync_cleans_reverse_direction_apply_if_present()
    {
        using var factory = new SVSimTestFactory();
        long target = await SeedViewer(factory, 76_561_198_000_001_011UL);
        long sender = await SeedViewer(factory, 76_561_198_000_001_012UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var a = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            // The reverse-direction apply that should get cleaned.
            var b = new ViewerFriendApply { FromViewerId = target, ToViewerId = sender, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.AddRange(a, b);
            await ctx.SaveChangesAsync();
            applyId = a.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.ApproveApplyAsync(target, applyId, default);

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await Ctx(verifyScope).ViewerFriendApplies.CountAsync(), Is.EqualTo(0),
            "Both directions' applies must be cleaned");
    }

    [Test]
    public async Task ApproveApplyAsync_no_op_when_apply_not_addressed_to_caller()
    {
        using var factory = new SVSimTestFactory();
        long imposter = await SeedViewer(factory, 76_561_198_000_001_013UL);
        long target = await SeedViewer(factory, 76_561_198_000_001_014UL);
        long sender = await SeedViewer(factory, 76_561_198_000_001_015UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.ApproveApplyAsync(imposter, applyId, default);

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = Ctx(verifyScope);
        Assert.That(await ctx2.ViewerFriendApplies.CountAsync(), Is.EqualTo(1), "Apply must survive");
        Assert.That(await ctx2.ViewerFriends.CountAsync(), Is.EqualTo(0), "No friendship created");
    }

    [Test]
    public async Task RejectApplyAsync_deletes_apply_only_for_correct_recipient()
    {
        using var factory = new SVSimTestFactory();
        long target = await SeedViewer(factory, 76_561_198_000_001_016UL);
        long sender = await SeedViewer(factory, 76_561_198_000_001_017UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.RejectApplyAsync(target, applyId, default);

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await Ctx(verifyScope).ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task CancelApplyAsync_deletes_apply_only_for_correct_sender()
    {
        using var factory = new SVSimTestFactory();
        long sender = await SeedViewer(factory, 76_561_198_000_001_018UL);
        long target = await SeedViewer(factory, 76_561_198_000_001_019UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.CancelApplyAsync(sender, applyId, default);

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await Ctx(verifyScope).ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RejectFriendAsync_deletes_both_directions()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_001_020UL);
        long other = await SeedViewer(factory, 76_561_198_000_001_021UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = me, FriendViewerId = other, CreatedAt = DateTime.UtcNow });
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = other, FriendViewerId = me, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.RejectFriendAsync(me, (int)other, default);

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await Ctx(verifyScope).ViewerFriends.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RejectAllAppliesAsync_deletes_only_incoming_for_caller()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_002_001UL);
        long other = await SeedViewer(factory, 76_561_198_000_002_002UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = other, ToViewerId = me, CreatedAt = DateTime.UtcNow });
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = me, ToViewerId = other, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.RejectAllAppliesAsync(me, default);

        using var verifyScope = factory.Services.CreateScope();
        var remaining = await Ctx(verifyScope).ViewerFriendApplies.AsNoTracking().ToListAsync();
        Assert.That(remaining, Has.Count.EqualTo(1));
        Assert.That(remaining[0].FromViewerId, Is.EqualTo(me), "Outgoing must survive");
    }

    [Test]
    public async Task CancelAllAppliesAsync_deletes_only_outgoing_for_caller()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_002_003UL);
        long other = await SeedViewer(factory, 76_561_198_000_002_004UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = Ctx(scope);
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = me, ToViewerId = other, CreatedAt = DateTime.UtcNow });
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = other, ToViewerId = me, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var svc = Service(factory, out var scope2);
        using (scope2) await svc.CancelAllAppliesAsync(me, default);

        using var verifyScope = factory.Services.CreateScope();
        var remaining = await Ctx(verifyScope).ViewerFriendApplies.AsNoTracking().ToListAsync();
        Assert.That(remaining, Has.Count.EqualTo(1));
        Assert.That(remaining[0].ToViewerId, Is.EqualTo(me), "Incoming must survive");
    }

    [Test]
    public async Task RecordAsync_upserts_PlayedAt_for_existing_pair()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_002_005UL);
        long opp = await SeedViewer(factory, 76_561_198_000_002_006UL);

        var ctxFactory = factory.Services;
        IPlayedTogetherWriter writer;
        using (var scope = ctxFactory.CreateScope())
        {
            writer = scope.ServiceProvider.GetRequiredService<IPlayedTogetherWriter>();
            await writer.RecordAsync(me, opp, new BattleParticipationContext(1, 2, 3, 4), default);
        }
        DateTime firstTimestamp;
        using (var scope = ctxFactory.CreateScope())
        {
            firstTimestamp = (await Ctx(scope).ViewerPlayedTogethers.AsNoTracking()
                .FirstAsync(p => p.OwnerViewerId == me && p.OpponentViewerId == opp)).PlayedAt;
        }

        // Wait a tick, record again.
        await Task.Delay(20);
        using (var scope = ctxFactory.CreateScope())
        {
            writer = scope.ServiceProvider.GetRequiredService<IPlayedTogetherWriter>();
            await writer.RecordAsync(me, opp, new BattleParticipationContext(5, 6, 7, 8), default);
        }

        using var verifyScope = ctxFactory.CreateScope();
        var rows = await Ctx(verifyScope).ViewerPlayedTogethers.AsNoTracking()
            .Where(p => p.OwnerViewerId == me).ToListAsync();
        Assert.That(rows, Has.Count.EqualTo(1), "Upsert — no duplicate row");
        Assert.That(rows[0].PlayedAt, Is.GreaterThan(firstTimestamp));
        Assert.That(rows[0].PlayedMode, Is.EqualTo(5), "Latest context wins");
        Assert.That(rows[0].BattleType, Is.EqualTo(6));
        Assert.That(rows[0].DeckFormat, Is.EqualTo(7));
        Assert.That(rows[0].TwoPickType, Is.EqualTo(8));
    }

    [Test]
    public async Task RecordAsync_no_op_when_owner_equals_opponent()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_002_007UL);

        using (var scope = factory.Services.CreateScope())
        {
            var writer = scope.ServiceProvider.GetRequiredService<IPlayedTogetherWriter>();
            await writer.RecordAsync(me, me, new BattleParticipationContext(0, 0, 0, 0), default);
        }

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await Ctx(verifyScope).ViewerPlayedTogethers.CountAsync(), Is.EqualTo(0));
    }
}
