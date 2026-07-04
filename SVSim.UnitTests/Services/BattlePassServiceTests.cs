using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.BattlePass;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class BattlePassServiceTests
{
    // Reward Id formula matches BattlePassRewardImporter.MakeId — keep in sync.
    private static long MakeRewardId(int seasonId, BattlePassTrack track, int level)
        => seasonId * 10_000L + (long)track * 1_000 + level;

    private static async Task<long> SeedViewerAndSeason23(SVSimTestFactory f, bool isPremium = false)
    {
        long viewerId = await f.SeedViewerAsync();
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        // Zero out rupees so post-state totals in reward assertions equal the delta amounts.
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.Currency.Rupees = 0;
        await db.SaveChangesAsync();
        if (await db.BattlePassLevels.CountAsync() == 0)
        {
            for (int i = 1; i <= 100; i++)
                db.BattlePassLevels.Add(new BattlePassLevelEntry { Level = i, RequiredPoint = (i - 1) * 500 });
        }
        db.BattlePassSeasons.Add(new BattlePassSeasonEntry
        {
            Id = 23, Name = "Season 23", MaxLevel = 100,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CanPurchase = true, PriceCrystal = 980, Description = "",
        });
        // Normal level-2 = rupy 50; premium level-2 = rupy 20.
        db.BattlePassRewards.Add(new BattlePassRewardEntry
        {
            Id = MakeRewardId(23, BattlePassTrack.Normal, 2),
            SeasonId = 23, Track = BattlePassTrack.Normal, Level = 2, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 50, IsAppealExclusion = false,
        });
        db.BattlePassRewards.Add(new BattlePassRewardEntry
        {
            Id = MakeRewardId(23, BattlePassTrack.Premium, 2),
            SeasonId = 23, Track = BattlePassTrack.Premium, Level = 2, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 20, IsAppealExclusion = false,
        });
        if (isPremium)
        {
            db.ViewerBattlePassProgress.Add(new ViewerBattlePassProgressEntry
            {
                ViewerId = viewerId, SeasonId = 23, IsPremium = true,
            });
        }
        await db.SaveChangesAsync();
        return viewerId;
    }

    [Test]
    public async Task AddPoints_crossing_one_level_grants_normal_reward_only_when_not_premium()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewerAndSeason23(factory, isPremium: false);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IBattlePassService>();

        var grant = await svc.AddPointsAsync(viewerId, BattlePassPointSource.BattleResult, 500, CancellationToken.None);

        Assert.That(grant.BeforeLevel, Is.EqualTo(1));
        Assert.That(grant.AfterLevel, Is.EqualTo(2));
        Assert.That(grant.PointAdd, Is.EqualTo(500));
        Assert.That(grant.NewlyClaimed.Count, Is.EqualTo(1), "premium level-2 must be skipped");
        Assert.That(grant.NewlyClaimed[0].RewardType, Is.EqualTo(UserGoodsType.Rupy));
        Assert.That(grant.NewlyClaimed[0].RewardNum, Is.EqualTo(50), "post-state rupy total");
    }

    [Test]
    public async Task AddPoints_with_premium_grants_both_tracks_at_level_crossed()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewerAndSeason23(factory, isPremium: true);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IBattlePassService>();

        var grant = await svc.AddPointsAsync(viewerId, BattlePassPointSource.BattleResult, 500, CancellationToken.None);

        Assert.That(grant.NewlyClaimed.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task AddPoints_weekly_cap_caps_second_grant_to_remaining_headroom()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewerAndSeason23(factory);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IBattlePassService>();

        var first = await svc.AddPointsAsync(viewerId, BattlePassPointSource.BattleResult, 2000, CancellationToken.None);
        var second = await svc.AddPointsAsync(viewerId, BattlePassPointSource.BattleResult, 2000, CancellationToken.None);

        Assert.That(first.PointAdd, Is.EqualTo(2000));
        Assert.That(second.PointAdd, Is.EqualTo(1000),
            $"cap = {BattlePassService.WeeklyLimitPointDefault}; first burned 2000; second caps to 1000");
    }

    [Test]
    public async Task AddPoints_when_no_season_active_returns_zero_grant()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IBattlePassService>();

        var grant = await svc.AddPointsAsync(viewerId, BattlePassPointSource.BattleResult, 500, CancellationToken.None);

        Assert.That(grant.PointAdd, Is.EqualTo(0));
        Assert.That(grant.AfterLevel, Is.EqualTo(0));
        Assert.That(grant.NewlyClaimed, Is.Empty);
    }
}
