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

/// <summary>
/// Tests for the four new shop endpoints (/products, /buy, /buy_set, /buy_set_item, /ids).
/// Existing /set tests live in the older smoke-test files; this class only covers the new
/// surface added with the leader-skin-shop family.
/// </summary>
public class LeaderSkinShopControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds one series (9001) with 2 products (skins 9101, 9102). Each product grants
    /// only its skin (no emblem/sleeve cascade — keeps the test self-contained without
    /// touching the cosmetic catalog). Set sale active at 800 crystals / 800 rupy.
    /// Set-completion bonus is 500 rupy.
    /// </summary>
    private static async Task SeedShop(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // LeaderSkin cosmetic catalog rows (RewardGrantService.AddCosmeticIfMissing looks these up)
        if (!await db.LeaderSkins.AnyAsync(s => s.Id == 9101))
            db.LeaderSkins.Add(new LeaderSkinEntry { Id = 9101, ClassId = 1 });
        if (!await db.LeaderSkins.AnyAsync(s => s.Id == 9102))
            db.LeaderSkins.Add(new LeaderSkinEntry { Id = 9102, ClassId = 1 });

        db.LeaderSkinShopSeries.Add(new LeaderSkinShopSeriesEntry
        {
            Id = 9001, IsEnabled = true, IsNew = false,
            SetSalesStatus = 1, SetPriceCrystal = 800, SetPriceRupy = 800,
            Products =
            {
                new LeaderSkinShopProductEntry
                {
                    Id = 90011, SeriesId = 9001, LeaderSkinId = 9101,
                    ProductNameKey = "LSPPN_test_1", IntroductionKey = "LSPI_test_1", CvNameKey = "LSPCN_test_1",
                    SinglePriceCrystal = 500, SinglePriceRupy = 500, IsEnabled = true,
                    Rewards = { new LeaderSkinShopProductRewardEntry { OrderIndex = 0, RewardType = (UserGoodsType)10, RewardDetailId = 9101, RewardNumber = 1 } },
                },
                new LeaderSkinShopProductEntry
                {
                    Id = 90012, SeriesId = 9001, LeaderSkinId = 9102,
                    ProductNameKey = "LSPPN_test_2", IntroductionKey = "LSPI_test_2", CvNameKey = "LSPCN_test_2",
                    SinglePriceCrystal = 500, SinglePriceRupy = 500, IsEnabled = true,
                    Rewards = { new LeaderSkinShopProductRewardEntry { OrderIndex = 0, RewardType = (UserGoodsType)10, RewardDetailId = 9102, RewardNumber = 1 } },
                },
            },
            SetCompletionRewards =
            {
                new LeaderSkinShopSeriesRewardEntry { OrderIndex = 0, RewardType = (UserGoodsType)9, RewardDetailId = 0, RewardNumber = 500 },
            },
        });
        await db.SaveChangesAsync();
    }

    private static async Task SetViewerCurrency(SVSimTestFactory f, long viewerId, ulong crystals = 0, ulong rupies = 0)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = crystals;
        v.Currency.Rupees = rupies;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Products_returns_dict_keyed_by_series_id_with_set_fields_emitted()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/leader_skin/products",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Object), "wire shape is dict-keyed by series_id string");

        var series = root.GetProperty("9001");
        Assert.That(series.GetProperty("series_id").GetInt32(), Is.EqualTo(9001));
        Assert.That(series.GetProperty("set_sales_status").GetInt32(), Is.EqualTo(1));
        Assert.That(series.GetProperty("set_prices").GetProperty("set_price_crystal").GetInt32(), Is.EqualTo(800));

        var products = series.GetProperty("products");
        Assert.That(products.GetArrayLength(), Is.EqualTo(2));
        Assert.That(products[0].GetProperty("is_purchased").GetBoolean(), Is.False);

        // set_completion bonus item should be in rewards.items
        var rewards = series.GetProperty("rewards");
        Assert.That(rewards.GetProperty("items").GetArrayLength(), Is.EqualTo(1));
    }

    [Test]
    public async Task Buy_single_crystal_debits_and_grants_skin()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);
        await SetViewerCurrency(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/leader_skin/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":90011,"sales_type":1,"item_id":null}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(2));   // crystal post-state + skin grant

        var crystal = rewardList[0];
        Assert.That(crystal.GetProperty("reward_type").GetInt32(), Is.EqualTo(2));
        Assert.That(crystal.GetProperty("reward_num").GetInt32(), Is.EqualTo(500));

        var skin = rewardList[1];
        Assert.That(skin.GetProperty("reward_type").GetInt32(), Is.EqualTo(10));
        Assert.That(skin.GetProperty("reward_id").GetInt64(), Is.EqualTo(9101));

        // Viewer should now own skin 9101
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
        Assert.That(v.LeaderSkins.Any(s => s.Id == 9101), Is.True);
    }

    [Test]
    public async Task Buy_already_purchased_skin_rejects_with_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);
        await SetViewerCurrency(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var first = await client.PostAsync("/leader_skin/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":90011,"sales_type":1,"item_id":null}"""));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var second = await client.PostAsync("/leader_skin/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":90011,"sales_type":1,"item_id":null}"""));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Buy_ticket_sales_type_returns_501()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/leader_skin/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":90011,"sales_type":3,"item_id":900001}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
    }

    [Test]
    public async Task BuySet_grants_all_skins_in_series()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);
        await SetViewerCurrency(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/leader_skin/buy_set",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"sales_type":1,"item_id":null}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(3));   // crystal post + skin1 + skin2

        // Viewer should own both skins
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
        Assert.That(v.LeaderSkins.Count(s => s.Id == 9101 || s.Id == 9102), Is.EqualTo(2));
        Assert.That(v.Currency.Crystals, Is.EqualTo(200UL));   // 1000 - 800 set price
    }

    [Test]
    public async Task BuySetItem_rejects_until_series_completed_then_succeeds()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);
        await SetViewerCurrency(factory, viewerId, rupies: 5000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // Without owning any skin: rejected
        var early = await client.PostAsync("/leader_skin/buy_set_item",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001}"""));
        Assert.That(early.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Buy both skins via buy_set
        var setBuy = await client.PostAsync("/leader_skin/buy_set",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"sales_type":2,"item_id":null}"""));
        Assert.That(setBuy.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Now claim succeeds, grants the bonus (500 rupy)
        var claim = await client.PostAsync("/leader_skin/buy_set_item",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001}"""));
        var body = await claim.Content.ReadAsStringAsync();
        Assert.That(claim.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(1));
        Assert.That(rewardList[0].GetProperty("reward_type").GetInt32(), Is.EqualTo(9)); // Rupy

        // Second claim returns OK with empty reward_list (idempotent — not 400)
        var second = await client.PostAsync("/leader_skin/buy_set_item",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001}"""));
        var secondBody = await second.Content.ReadAsStringAsync();
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc2 = JsonDocument.Parse(secondBody);
        Assert.That(doc2.RootElement.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Ids_returns_owned_leader_skin_ids()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedShop(factory);
        await SetViewerCurrency(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // Initial state: no owned skins from our shop
        var beforeResp = await client.PostAsync("/leader_skin/ids",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var beforeBody = await beforeResp.Content.ReadAsStringAsync();
        using var beforeDoc = JsonDocument.Parse(beforeBody);
        bool ownsBefore = false;
        foreach (var id in beforeDoc.RootElement.GetProperty("user_leader_skin_ids").EnumerateArray())
            if (id.GetInt32() == 9101) { ownsBefore = true; break; }
        Assert.That(ownsBefore, Is.False);

        // Buy skin 9101
        await client.PostAsync("/leader_skin/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":90011,"sales_type":1,"item_id":null}"""));

        var afterResp = await client.PostAsync("/leader_skin/ids",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var afterBody = await afterResp.Content.ReadAsStringAsync();
        using var afterDoc = JsonDocument.Parse(afterBody);
        bool ownsAfter = false;
        foreach (var id in afterDoc.RootElement.GetProperty("user_leader_skin_ids").EnumerateArray())
            if (id.GetInt32() == 9101) { ownsAfter = true; break; }
        Assert.That(ownsAfter, Is.True);
    }

    [Test]
    public async Task BuySet_on_series_without_set_sale_rejects()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.LeaderSkinShopSeries.Add(new LeaderSkinShopSeriesEntry
            {
                Id = 9999, IsEnabled = true, SetSalesStatus = 0,  // no set sale
                Products = { new LeaderSkinShopProductEntry { Id = 99991, SeriesId = 9999, LeaderSkinId = 1, IsEnabled = true, SinglePriceCrystal = 500 } },
            });
            await db.SaveChangesAsync();
        }
        await SetViewerCurrency(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/leader_skin/buy_set",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9999,"sales_type":1,"item_id":null}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
