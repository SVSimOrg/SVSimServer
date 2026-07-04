using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class PackControllerGachaPointTests
{
    private static StringContent JsonBody(string json) =>
        new(json, Encoding.UTF8, "application/json");

    [Test]
    public async Task GetGachaPointRewards_returns_catalog_for_active_pack()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
            db.CardSets.Add(set);
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
                Class = db.Classes.Local.First(), IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
            });
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""{"odds_gacha_id":10008,"parent_gacha_id":10008,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/get_gacha_point_rewards", body);
        var text = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var rewards = doc.RootElement.GetProperty("gacha_point_rewards");
        Assert.That(rewards.GetArrayLength(), Is.EqualTo(1));
        var entry = rewards[0];
        Assert.That(entry.GetProperty("class_id").GetString(), Is.EqualTo("0"),
            "class_id must be wire-typed as a string");
        Assert.That(entry.GetProperty("card_id").GetInt64(), Is.EqualTo(108041010));
        Assert.That(entry.GetProperty("is_received").GetBoolean(), Is.False);
        var rewardList = entry.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(1));
        Assert.That(rewardList[0].GetProperty("reward_type").GetInt32(), Is.EqualTo(7));
        Assert.That(rewardList[0].GetProperty("reward_detail_id").GetInt64(), Is.EqualTo(1080410100));
        Assert.That(rewardList[0].GetProperty("reward_number").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetGachaPointRewards_uses_odds_gacha_id_when_it_differs_from_parent_gacha_id()
    {
        // Regression for the seasonal-pack case captured in traffic_prod_all_gacha_exchange.ndjson:
        // the client sends {odds_gacha_id: 16xxx, parent_gacha_id: 10xxx} where odds_gacha_id is
        // the active seasonal pack (carries GachaPointConfig + balance) and parent_gacha_id is
        // the base/family pack id (often a disabled stub in our DB). Looking up by parent_gacha_id
        // would land on the stub and return [].
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var set = new ShadowverseCardSetEntry { Id = 16015, IsInRotation = true };
            db.CardSets.Add(set);
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 115041010, Name = "ucl-leg", Rarity = Rarity.Legendary,
                Class = db.Classes.Local.First(), IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = 115041010, Type = CosmeticType.Emblem, CosmeticId = 1150410100,
            });
            // Active seasonal pack — has GachaPointConfig.
            db.Packs.Add(new PackConfigEntry
            {
                Id = 16015, BasePackId = 10015, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "UCL season",
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            // Disabled base/family stub — no GachaPointConfig (matches the synthesized-stub state).
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10015, BasePackId = 10015, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "UCL family stub", IsEnabled = false,
                GachaPointConfig = null,
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableFromSetAsync(16015, 16015);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // Mirrors the prod capture shape — odds_gacha_id is the active pack, parent_gacha_id
        // is the base/family id (here, a disabled stub).
        var body = JsonBody("""{"odds_gacha_id":16015,"parent_gacha_id":10015,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/get_gacha_point_rewards", body);

        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var rewards = doc.RootElement.GetProperty("gacha_point_rewards");
        Assert.That(rewards.GetArrayLength(), Is.EqualTo(1),
            "lookup must resolve via odds_gacha_id (16015), not parent_gacha_id (10015)");
        Assert.That(rewards[0].GetProperty("card_id").GetInt64(), Is.EqualTo(115041010));
    }

    [Test]
    public async Task GetGachaPointRewards_includes_gold_tier_leader_cards()
    {
        // Regression for the UCL pack 16015 case captured in traffic_prod_all_gacha_exchange.ndjson:
        // leader cards Kyoka (711531010, Runecraft) and Miyako (711331010, Dragoncraft) show up
        // in the prod /pack/get_gacha_point_rewards response despite being Gold tier rather than
        // Legendary. The exchange filter must include IsLeader=true cards regardless of their
        // page tier.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        const long GoldLeaderCardId = 711531010;
        const int  LeaderSkinId      = 1805;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var classRune = db.Classes.Local.First();
            var set = new ShadowverseCardSetEntry { Id = 16015, IsInRotation = true };
            db.CardSets.Add(set);
            // Intrinsic Rarity here is Legendary (matches card master for these leaders),
            // but in the pack-draw table the row carries Tier=Gold + IsLeader=true.
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = GoldLeaderCardId, Name = "Kyoka, Prize Pupil",
                Rarity = Rarity.Legendary, Class = classRune, IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = GoldLeaderCardId, Type = CosmeticType.Skin, CosmeticId = LeaderSkinId,
            });
            db.Packs.Add(new PackConfigEntry
            {
                Id = 16015, BasePackId = 10015, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "UCL",
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            await db.SaveChangesAsync();

            // Install the draw table manually so the Gold-tier IsLeader=true flags land
            // exactly the way the extractor would emit them.
            db.PackDrawConfigs.Add(new PackDrawConfigEntry { Id = 16015, AnimationRatePct = 0 });
            db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry
            {
                PackId = 16015, Slot = DrawSlot.General, Tier = DrawTier.Gold, RatePct = 100,
            });
            db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry
            {
                PackId = 16015, Slot = DrawSlot.General, Tier = DrawTier.Gold,
                CardId = GoldLeaderCardId, RatePct = 0.12, IsLeader = true, IsAltArt = false,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""{"odds_gacha_id":16015,"parent_gacha_id":10015,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/get_gacha_point_rewards", body);

        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var rewards = doc.RootElement.GetProperty("gacha_point_rewards");
        Assert.That(rewards.GetArrayLength(), Is.EqualTo(1),
            "Gold-tier leader card must appear in the exchange catalog");
        var entry = rewards[0];
        Assert.That(entry.GetProperty("card_id").GetInt64(), Is.EqualTo(GoldLeaderCardId));
        // Reward list must include the Skin row (type=10) — that's the leader-skin reward
        // which is the whole reason this card is exchangeable.
        var rewardList = entry.GetProperty("reward_list");
        var skinEntry = Enumerable.Range(0, rewardList.GetArrayLength())
            .Select(i => rewardList[i])
            .FirstOrDefault(e => e.GetProperty("reward_type").GetInt32() == 10);
        Assert.That(skinEntry.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined),
            "leader card entry should carry a Skin reward");
        Assert.That(skinEntry.GetProperty("reward_detail_id").GetInt64(), Is.EqualTo(LeaderSkinId));
    }

    [Test]
    public async Task GetGachaPointRewards_wire_keys_match_prod_capture()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
            db.CardSets.Add(set);
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
                Class = db.Classes.Local.First(), IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
            });
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""{"odds_gacha_id":10008,"parent_gacha_id":10008,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/get_gacha_point_rewards", body);
        var text = await response.Content.ReadAsStringAsync();

        // Literal wire-key checks — verified against
        // data_dumps/captures/traffic_prod_tradeables_capture.ndjson pack 10008 response.
        Assert.That(text, Does.Contain("\"gacha_point_rewards\""));
        Assert.That(text, Does.Contain("\"class_id\":\"0\""), "class_id MUST be a string");
        Assert.That(text, Does.Contain("\"reward_detail_id\":1080410100"),
            "per-card entry uses reward_detail_id (not reward_id)");
        Assert.That(text, Does.Contain("\"reward_number\":1"),
            "per-card entry uses reward_number (not reward_num)");
        Assert.That(text, Does.Contain("\"is_received\":false"));
        Assert.That(text, Does.Contain("\"is_display_prize\":false"));
    }

    [Test]
    public async Task ExchangeGachaPoint_grants_card_and_returns_post_state_reward_list()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
            db.CardSets.Add(set);
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
                Class = db.Classes.Local.First(), IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
            });
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            var viewer = await db.Viewers
                .Include(v => v.GachaPointBalances)
                .FirstAsync(v => v.Id == viewerId);
            viewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 500 });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""{"card_id":108041010,"parent_gacha_id":10008,"odds_gacha_id":10008,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/exchange_gacha_point", body);
        var text = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.GreaterThan(0));
        // Verify the card grant entry (type=5/Card) is present with the granted card id.
        bool foundCard = false;
        foreach (var r in rewardList.EnumerateArray())
        {
            if (r.GetProperty("reward_type").GetInt32() == 5 &&
                r.GetProperty("reward_id").GetInt64() == 108041010)
            {
                foundCard = true;
                break;
            }
        }
        Assert.That(foundCard, Is.True, "card grant entry missing from reward_list");

        // Verify side-effects.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers
                .Include(v => v.GachaPointBalances)
                .Include(v => v.GachaPointReceived)
                .Include(v => v.Cards).ThenInclude(c => c.Card)
                .AsSplitQuery()
                .FirstAsync(v => v.Id == viewerId);
            Assert.That(viewer.GachaPointBalances.Single().Points, Is.EqualTo(100));
            Assert.That(viewer.GachaPointReceived.Single().CardId, Is.EqualTo(108041010));
            Assert.That(viewer.Cards.Any(c => c.Card.Id == 108041010), Is.True);
        }
    }

    [Test]
    public async Task ExchangeGachaPoint_rejects_when_balance_insufficient()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Classes.Add(new ClassEntry { Id = 0, Name = "Neutral" });
            var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
            db.CardSets.Add(set);
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
                Class = db.Classes.Local.First(), IsFoil = false,
            });
            db.CardCosmeticRewards.Add(new CardCosmeticReward
            {
                CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
            });
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""{"card_id":108041010,"parent_gacha_id":10008,"odds_gacha_id":10008,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/pack/exchange_gacha_point", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            await response.Content.ReadAsStringAsync());
    }
}
