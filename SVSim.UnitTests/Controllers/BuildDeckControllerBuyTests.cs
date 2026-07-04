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

public class BuildDeckControllerBuyTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds: series 101 (enabled), one crystal-priced product 1 (intro=500/regular=750, max=3)
    /// containing 2 distinct cards (10001001 ×2, 10001002 ×1). Caller may set viewer crystals.
    /// </summary>
    private static async Task SeedCrystalProduct(SVSimTestFactory f, long viewerId, ulong crystals)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
        {
            Id = 101, OrderIndex = 22, IsEnabled = true, NameKey = "BDSSN_test", IntroKey = "BDSI_test",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 1, SeriesId = 101, LeaderId = 1, DeckCode = "pd0101",
                    PurchaseNumMax = 3, IntroPriceCrystal = 500, RegularPriceCrystal = 750,
                    IsEnabled = true,
                    Cards =
                    {
                        new BuildDeckProductCardEntry { CardId = 10001001L, Number = 2, IsSpot = false },
                        new BuildDeckProductCardEntry { CardId = 10001002L, Number = 1, IsSpot = false },
                    },
                },
            },
        });
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = crystals;
        await db.SaveChangesAsync();
    }

    private static async Task SeedRupyProduct(SVSimTestFactory f, long viewerId, ulong rupees)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
        {
            Id = 102, OrderIndex = 23, IsEnabled = true, NameKey = "BDSSN_rupy", IntroKey = "BDSI_rupy",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 10, SeriesId = 102, LeaderId = 2, DeckCode = "pdR",
                    PurchaseNumMax = 1, IntroPriceRupy = 100,
                    IsEnabled = true,
                    Cards = { new BuildDeckProductCardEntry { CardId = 10001001L, Number = 1, IsSpot = false } },
                },
            },
        });
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Rupees = rupees;
        await db.SaveChangesAsync();
    }

    private static async Task SeedFreeProduct(SVSimTestFactory f, long viewerId)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
        {
            Id = 103, OrderIndex = 24, IsEnabled = true, NameKey = "BDSSN_free", IntroKey = "BDSI_free",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 20, SeriesId = 103, LeaderId = 3, DeckCode = "pdF",
                    PurchaseNumMax = 1, IntroPriceCrystal = 0, IntroPriceRupy = 0,
                    IsEnabled = true,
                    Cards = { new BuildDeckProductCardEntry { CardId = 10001003L, Number = 1, IsSpot = false } },
                },
            },
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds: series 104 + product 100 with a per-buy sleeve reward (id 3000021, a real seeded
    /// sleeve master row). Used to verify the per-buy rewards path that drops sleeve/emblem/skin
    /// grants if the controller's Rewards iteration is missing.
    /// </summary>
    private static async Task SeedProductWithSleeveReward(SVSimTestFactory f, long viewerId)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
        {
            Id = 104, OrderIndex = 19, IsEnabled = true, NameKey = "BDSSN_sleeve", IntroKey = "BDSI_sleeve",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 100, SeriesId = 104, LeaderId = 1, DeckCode = "pd0104",
                    PurchaseNumMax = 1, IntroPriceCrystal = 0, IntroPriceRupy = 0,   // free
                    IsEnabled = true,
                    Cards = { new BuildDeckProductCardEntry { CardId = 10001001L, Number = 1, IsSpot = false } },
                    Rewards =
                    {
                        new BuildDeckProductRewardEntry
                        {
                            RewardIndex = 1, RewardType = (UserGoodsType)6 /* Sleeve */,
                            RewardDetailId = 3000021, RewardNumber = 1, MessageId = 51004,
                        },
                    },
                },
            },
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Buy_grants_per_buy_sleeve_reward_to_viewer_collection()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedProductWithSleeveReward(factory, viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":100,"sales_type":0}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.Sleeves).FirstAsync(x => x.Id == viewerId);
        Assert.That(v.Sleeves.Any(s => s.Id == 3000021), Is.True,
            "per-buy sleeve reward must land in viewer's owned collection");

        using var doc = JsonDocument.Parse(body);
        var entries = doc.RootElement.GetProperty("reward_list");
        bool foundSleeve = false;
        for (int i = 0; i < entries.GetArrayLength(); i++)
        {
            var e = entries[i];
            if (e.GetProperty("reward_type").GetInt32() == 6 && e.GetProperty("reward_id").GetInt64() == 3000021)
                foundSleeve = true;
        }
        Assert.That(foundSleeve, Is.True, "reward_list must include the granted sleeve entry");
    }

    [Test]
    public async Task Crystal_buy_debits_intro_price_and_grants_cards()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":1}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers
            .Include(x => x.Cards).ThenInclude(c => c.Card)
            .Include(x => x.BuildDeckPurchases)
            .FirstAsync(x => x.Id == viewerId);

        Assert.That(v.Currency.Crystals, Is.EqualTo(500UL), "1000 - 500 intro");
        Assert.That(v.Cards.Sum(c => c.Count), Is.EqualTo(3), "2 + 1 cards granted");
        Assert.That(v.BuildDeckPurchases.Single(p => p.ProductId == 1).PurchaseCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Crystal_buy_emits_post_state_total_for_crystals()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":1}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        bool foundCrystals = false;
        for (int i = 0; i < rewardList.GetArrayLength(); i++)
        {
            var e = rewardList[i];
            if (e.GetProperty("reward_type").GetInt32() == 2)
            {
                Assert.That(e.GetProperty("reward_num").GetInt32(), Is.EqualTo(500), "post-state crystals total");
                foundCrystals = true;
            }
        }
        Assert.That(foundCrystals, Is.True, "crystal entry must be in reward_list");
    }

    [Test]
    public async Task Returns_BadRequest_when_insufficient_crystals()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 100);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":1}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Returns_BadRequest_for_disabled_product()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
            {
                Id = 101, OrderIndex = 22, IsEnabled = true, NameKey = "x", IntroKey = "x",
                Products =
                {
                    new BuildDeckProductEntry
                    {
                        Id = 999, SeriesId = 101, PurchaseNumMax = 1, IntroPriceCrystal = 500,
                        IsEnabled = false,
                    },
                },
            });
            await db.SaveChangesAsync();
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":999,"sales_type":1}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Returns_BadRequest_when_purchase_limit_reached()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 10000);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.BuildDeckPurchases).FirstAsync(x => x.Id == viewerId);
            v.BuildDeckPurchases.Add(new ViewerBuildDeckProductPurchase { ProductId = 1, PurchaseCount = 3 });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":1}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Returns_BadRequest_when_paying_in_unsupported_currency_for_product()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);   // crystal-only product

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // sales_type=2 (rupy) against a crystal-only product
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":2}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Returns_501_for_ticket_sales_type()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":3}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That((int)response.StatusCode, Is.EqualTo(501));
    }

    [Test]
    public async Task Rupy_buy_debits_and_grants()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedRupyProduct(factory, viewerId, rupees: 200);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":10,"sales_type":2}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.Cards).FirstAsync(x => x.Id == viewerId);
        Assert.That(v.Currency.Rupees, Is.EqualTo(100UL));

        using var doc = JsonDocument.Parse(body);
        var entries = doc.RootElement.GetProperty("reward_list");
        bool foundRupy = false;
        for (int i = 0; i < entries.GetArrayLength(); i++)
        {
            if (entries[i].GetProperty("reward_type").GetInt32() == 9)
            {
                Assert.That(entries[i].GetProperty("reward_num").GetInt32(), Is.EqualTo(100));
                foundRupy = true;
            }
        }
        Assert.That(foundRupy, Is.True);
    }

    [Test]
    public async Task Free_buy_grants_cards_without_currency_entry()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedFreeProduct(factory, viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":20,"sales_type":0}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var entries = doc.RootElement.GetProperty("reward_list");
        for (int i = 0; i < entries.GetArrayLength(); i++)
        {
            int t = entries[i].GetProperty("reward_type").GetInt32();
            Assert.That(t, Is.Not.EqualTo(2), "free buy must not emit Crystal entry");
            Assert.That(t, Is.Not.EqualTo(9), "free buy must not emit Rupy entry");
        }
    }

    [Test]
    public async Task Free_buy_against_nonfree_product_returns_BadRequest()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":1,"sales_type":0}""";
        var response = await client.PostAsync("/build_deck/buy", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Buy_emits_newly_unlocked_series_tier_rewards()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
            {
                Id = 105, OrderIndex = 18, IsEnabled = true, NameKey = "x", IntroKey = "x",
                SeriesRewards =
                {
                    // Tier 1: one card reward, unlocked on the 1st series purchase.
                    new BuildDeckSeriesRewardEntry
                    {
                        TierIndex = 1, ItemIndex = 0, RewardType = (UserGoodsType)5,
                        RewardDetailId = 10001001L, RewardNumber = 1, MessageId = 51004,
                    },
                    // Tier 2: one card reward, unlocked on the 2nd series purchase.
                    new BuildDeckSeriesRewardEntry
                    {
                        TierIndex = 2, ItemIndex = 0, RewardType = (UserGoodsType)5,
                        RewardDetailId = 10001002L, RewardNumber = 1, MessageId = 51004,
                    },
                },
                Products =
                {
                    new BuildDeckProductEntry
                    {
                        Id = 501, SeriesId = 105, LeaderId = 1, DeckCode = "pd0501",
                        PurchaseNumMax = 3, IntroPriceCrystal = 0, RegularPriceCrystal = 0,
                        IntroPriceRupy = 0, RegularPriceRupy = 0, IsEnabled = true,
                        Cards = { new BuildDeckProductCardEntry { CardId = 10001003L, Number = 1, IsSpot = false } },
                    },
                    new BuildDeckProductEntry
                    {
                        Id = 502, SeriesId = 105, LeaderId = 2, DeckCode = "pd0502",
                        PurchaseNumMax = 3, IntroPriceCrystal = 0, RegularPriceCrystal = 0,
                        IntroPriceRupy = 0, RegularPriceRupy = 0, IsEnabled = true,
                        Cards = { new BuildDeckProductCardEntry { CardId = 10001003L, Number = 1, IsSpot = false } },
                    },
                },
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // 1st series purchase (product 501) should emit tier 1 only.
        var r1 = await client.PostAsync("/build_deck/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":501,"sales_type":0}"""));
        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK), await r1.Content.ReadAsStringAsync());

        using (var doc = JsonDocument.Parse(await r1.Content.ReadAsStringAsync()))
        {
            var tiers = doc.RootElement.GetProperty("series_rewards");
            Assert.That(tiers.GetArrayLength(), Is.EqualTo(1), "only tier 1 newly crossed");
            Assert.That(tiers[0].GetProperty("reward_detail_id").GetInt64(), Is.EqualTo(10001001L));
        }

        // 2nd series purchase (product 502) should emit tier 2 only.
        var r2 = await client.PostAsync("/build_deck/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":502,"sales_type":0}"""));
        using (var doc = JsonDocument.Parse(await r2.Content.ReadAsStringAsync()))
        {
            var tiers = doc.RootElement.GetProperty("series_rewards");
            Assert.That(tiers.GetArrayLength(), Is.EqualTo(1));
            Assert.That(tiers[0].GetProperty("reward_detail_id").GetInt64(), Is.EqualTo(10001002L));
        }
    }
}
