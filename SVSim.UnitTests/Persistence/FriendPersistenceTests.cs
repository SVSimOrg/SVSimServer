using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Persistence;

public class FriendPersistenceTests
{
    [Test]
    public async Task ViewerFriend_round_trips_composite_PK_row()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL);

        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerFriends.Add(new ViewerFriend
            {
                OwnerViewerId = viewerA,
                FriendViewerId = viewerB,
                CreatedAt = new DateTime(2026, 6, 9, 12, 0, 0, DateTimeKind.Utc),
            });
            await ctx.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var row = await ctx2.ViewerFriends.AsNoTracking()
            .FirstAsync(f => f.OwnerViewerId == viewerA && f.FriendViewerId == viewerB);
        Assert.That(row.CreatedAt.Year, Is.EqualTo(2026));
    }

    [Test]
    public async Task ViewerFriend_composite_PK_rejects_duplicate_pair()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_003UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_004UL);

        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerA, FriendViewerId = viewerB, CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        using var dupScope = factory.Services.CreateScope();
        var dupCtx = dupScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        dupCtx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerA, FriendViewerId = viewerB, CreatedAt = DateTime.UtcNow });
        Assert.That(async () => await dupCtx.SaveChangesAsync(), Throws.Exception);
    }

    [Test]
    public async Task ViewerFriendApply_unique_constraint_rejects_duplicate_from_to_pair()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_005UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_006UL);

        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        ctx.ViewerFriendApplies.Add(new ViewerFriendApply
        {
            FromViewerId = viewerA, ToViewerId = viewerB, CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        using var dupScope = factory.Services.CreateScope();
        var dupCtx = dupScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        dupCtx.ViewerFriendApplies.Add(new ViewerFriendApply
        {
            FromViewerId = viewerA, ToViewerId = viewerB, CreatedAt = DateTime.UtcNow,
        });
        Assert.That(async () => await dupCtx.SaveChangesAsync(), Throws.Exception);
    }

    [Test]
    public async Task ViewerPlayedTogether_round_trips_all_columns()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_007UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_008UL);

        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = viewerA,
                OpponentViewerId = viewerB,
                PlayedAt = new DateTime(2026, 6, 9, 12, 0, 0, DateTimeKind.Utc),
                PlayedMode = 1,
                BattleType = 2,
                DeckFormat = 3,
                TwoPickType = 4,
            });
            await ctx.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var row = await ctx2.ViewerPlayedTogethers.AsNoTracking()
            .FirstAsync(p => p.OwnerViewerId == viewerA && p.OpponentViewerId == viewerB);
        Assert.That(row.PlayedMode, Is.EqualTo(1));
        Assert.That(row.BattleType, Is.EqualTo(2));
        Assert.That(row.DeckFormat, Is.EqualTo(3));
        Assert.That(row.TwoPickType, Is.EqualTo(4));
    }
}
