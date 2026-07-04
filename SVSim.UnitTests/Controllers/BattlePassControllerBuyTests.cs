using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class BattlePassControllerBuyTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    // Reward Id formula matches BattlePassRewardImporter.MakeId — keep in sync.
    private static long MakeRewardId(int seasonId, BattlePassTrack track, int level)
        => seasonId * 10_000L + (long)track * 1_000 + level;

    private static async Task SeedSeason23WithPremiumReward(SVSimTestFactory f, long viewerId, ulong viewerCrystals, int currentPoint)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
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
        db.BattlePassRewards.Add(new BattlePassRewardEntry
        {
            Id = MakeRewardId(23, BattlePassTrack.Premium, 2),
            SeasonId = 23, Track = BattlePassTrack.Premium, Level = 2, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 20, IsAppealExclusion = false,
        });
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = viewerCrystals;
        await db.SaveChangesAsync();

        if (currentPoint > 0)
        {
            var p = new ViewerBattlePassProgressEntry
            {
                ViewerId = viewerId, SeasonId = 23, CurrentPoint = currentPoint,
                IsPremium = false, WeeklyPoints = 0,
            };
            db.ViewerBattlePassProgress.Add(p);
            await db.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Buy_happy_path_at_level_1_deducts_crystals_and_emits_empty_achieved()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithPremiumReward(factory, viewerId, viewerCrystals: 1000, currentPoint: 0);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","season_id":23,"id":23000}""";
        var response = await client.PostAsync("/battle_pass/buy", JsonBody(req));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var achievedList = doc.RootElement.GetProperty("achieved_info").GetProperty("battle_pass_reward_list");
        Assert.That(achievedList.GetArrayLength(), Is.EqualTo(0),
            "level 1 → no premium rewards crossed");

        bool foundCrystal = false;
        foreach (var el in doc.RootElement.GetProperty("reward_list").EnumerateArray())
        {
            if (el.GetProperty("reward_type").GetInt32() == 2)
            {
                foundCrystal = true;
                Assert.That(el.GetProperty("reward_num").GetInt32(), Is.EqualTo(20),
                    "post-state crystal total = 1000 - 980");
            }
        }
        Assert.That(foundCrystal, Is.True, "reward_list must carry post-state crystal total");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        var progress = await db.ViewerBattlePassProgress.SingleAsync(p => p.ViewerId == viewerId);
        Assert.That(v.Currency.Crystals, Is.EqualTo(20UL));
        Assert.That(progress.IsPremium, Is.True);
    }

    [Test]
    public async Task Buy_at_level_2_includes_retroactive_premium_reward_in_achieved()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithPremiumReward(factory, viewerId, viewerCrystals: 1000, currentPoint: 500); // level 2

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","season_id":23,"id":23000}""";
        var response = await client.PostAsync("/battle_pass/buy", JsonBody(req));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var achievedList = doc.RootElement.GetProperty("achieved_info").GetProperty("battle_pass_reward_list");
        Assert.That(achievedList.GetArrayLength(), Is.EqualTo(1), "premium level-2 reward must drop");
        Assert.That(achievedList[0].GetProperty("reward_type").GetInt32(), Is.EqualTo(9));
        Assert.That(achievedList[0].GetProperty("reward_number").GetInt32(), Is.EqualTo(20));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        bool claimWritten = await db.ViewerBattlePassClaims.AnyAsync(
            c => c.ViewerId == viewerId && c.SeasonId == 23
                 && c.Track == BattlePassTrack.Premium && c.Level == 2);
        Assert.That(claimWritten, Is.True, "claim row must be persisted");
    }

    [Test]
    public async Task Buy_with_insufficient_crystals_returns_22_and_makes_no_changes()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithPremiumReward(factory, viewerId, viewerCrystals: 100, currentPoint: 0);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","season_id":23,"id":23000}""";
        var response = await client.PostAsync("/battle_pass/buy", JsonBody(req));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(22));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        Assert.That(v.Currency.Crystals, Is.EqualTo(100UL), "no crystal deduction on failure");
        bool anyProgress = await db.ViewerBattlePassProgress
            .AnyAsync(p => p.ViewerId == viewerId && p.IsPremium);
        Assert.That(anyProgress, Is.False);
    }

    [Test]
    public async Task Buy_when_already_premium_returns_23()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithPremiumReward(factory, viewerId, viewerCrystals: 5000, currentPoint: 0);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var p = await db.ViewerBattlePassProgress.SingleOrDefaultAsync(x => x.ViewerId == viewerId)
                    ?? new ViewerBattlePassProgressEntry { ViewerId = viewerId, SeasonId = 23 };
            p.IsPremium = true;
            if (p.Id == 0) db.ViewerBattlePassProgress.Add(p);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","season_id":23,"id":23000}""";
        var response = await client.PostAsync("/battle_pass/buy", JsonBody(req));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(23));
    }

    [Test]
    public async Task Buy_outside_period_returns_24()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // No season seeded → active-season lookup returns null.

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","season_id":23,"id":23000}""";
        var response = await client.PostAsync("/battle_pass/buy", JsonBody(req));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(24));
    }
}
