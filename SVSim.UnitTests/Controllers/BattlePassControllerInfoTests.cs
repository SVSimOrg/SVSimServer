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

public class BattlePassControllerInfoTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private const string EmptyAuthBody = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private static async Task SeedSeason23WithRewards(SVSimTestFactory f)
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
            CanPurchase = true, PriceCrystal = 980, Description = "test",
        });
        db.BattlePassRewards.Add(new BattlePassRewardEntry
        {
            Id = 23 * 10_000L + 0 * 1_000 + 2,  // MakeId(23, Normal=0, 2)
            SeasonId = 23, Track = BattlePassTrack.Normal, Level = 2, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 50, IsAppealExclusion = false,
        });
        db.BattlePassRewards.Add(new BattlePassRewardEntry
        {
            Id = 23 * 10_000L + 1 * 1_000 + 2,  // MakeId(23, Premium=1, 2)
            SeasonId = 23, Track = BattlePassTrack.Premium, Level = 2, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 20, IsAppealExclusion = false,
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Info_returns_season_with_zero_progress_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithRewards(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/info", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("season_info").GetProperty("id").GetString(), Is.EqualTo("23"));
        Assert.That(root.GetProperty("season_info").GetProperty("max_level").GetString(), Is.EqualTo("100"));
        Assert.That(root.GetProperty("season_info").GetProperty("can_purchase").GetBoolean(), Is.True);
        Assert.That(root.GetProperty("gauge_info").GetProperty("current_point").GetString(), Is.EqualTo("0"));
        Assert.That(root.GetProperty("gauge_info").GetProperty("current_level").GetString(), Is.EqualTo("1"));

        var normalRewards = root.GetProperty("reward_info").GetProperty("normal").GetProperty("reward");
        Assert.That(normalRewards.GetArrayLength(), Is.EqualTo(1));
        Assert.That(normalRewards[0].GetProperty("reward_level").GetString(), Is.EqualTo("2"));
        Assert.That(normalRewards[0].GetProperty("is_received").GetBoolean(), Is.False);
    }

    [Test]
    public async Task Info_marks_claimed_rewards_as_received()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithRewards(factory);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattlePassClaims.Add(new ViewerBattlePassClaimEntry
            {
                ViewerId = viewerId, SeasonId = 23, Track = BattlePassTrack.Normal,
                Level = 2, ClaimedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/info", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var normalReward = doc.RootElement
            .GetProperty("reward_info").GetProperty("normal").GetProperty("reward")[0];
        var premiumReward = doc.RootElement
            .GetProperty("reward_info").GetProperty("premium").GetProperty("reward")[0];
        Assert.That(normalReward.GetProperty("is_received").GetBoolean(), Is.True);
        Assert.That(premiumReward.GetProperty("is_received").GetBoolean(), Is.False);
    }

    [Test]
    public async Task Info_can_purchase_is_false_when_viewer_already_premium()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23WithRewards(factory);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattlePassProgress.Add(new ViewerBattlePassProgressEntry
            {
                ViewerId = viewerId, SeasonId = 23, CurrentPoint = 0, IsPremium = true,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/info", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.That(doc.RootElement.GetProperty("season_info").GetProperty("can_purchase").GetBoolean(),
            Is.False,
            "premium owners must see can_purchase=false (client uses it as the sole hide-buy-button signal)");
    }

    [Test]
    public async Task Info_returns_empty_payload_outside_any_season_window()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // No season seeded.

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/info", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(body, Does.Not.Contain("season_info").Or.Contains("\"season_info\":null"),
            "off-season response should omit or null season_info");
    }
}
