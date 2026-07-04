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

public class SleeveControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds: series 9001 (enabled) with one crystal-priced product 900101 granting
    /// sleeve 9000011 + emblem 9000011. Caller sets viewer crystals.
    /// Sleeve + emblem catalog rows are inserted with placeholder names so RewardGrantService
    /// can resolve them.
    /// </summary>
    private static async Task SeedCrystalProduct(SVSimTestFactory f, long viewerId, ulong crystals)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Sleeve + emblem catalog must exist so RewardGrantService.ApplyAsync can find them.
        // Using ids outside the 1k-1.6k cosmetic seed range so they can't collide with reference data.
        const int testSleeveId = 9000011;
        const int testEmblemId = 9000011;
        if (!await db.Sleeves.AnyAsync(s => s.Id == testSleeveId))
            db.Sleeves.Add(new SleeveEntry { Id = testSleeveId });
        if (!await db.Emblems.AnyAsync(e => e.Id == testEmblemId))
            db.Emblems.Add(new EmblemEntry { Id = testEmblemId });

        db.SleeveShopSeries.Add(new SleeveShopSeriesEntry
        {
            Id = 9001, IsEnabled = true, IsNew = false,
            Products =
            {
                new SleeveShopProductEntry
                {
                    Id = 900101, SeriesId = 9001, NameKey = "sleeve_test", PriceCrystal = 400,
                    IsEnabled = true,
                    Rewards =
                    {
                        new SleeveShopProductRewardEntry { OrderIndex = 0, RewardType = (UserGoodsType)7, RewardDetailId = testEmblemId, RewardNumber = 1 },
                        new SleeveShopProductRewardEntry { OrderIndex = 1, RewardType = (UserGoodsType)6, RewardDetailId = testSleeveId, RewardNumber = 1 },
                    },
                },
            },
        });

        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = crystals;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Info_returns_dict_keyed_by_series_id_and_product_id()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 0);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/sleeve/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var sleeveList = doc.RootElement.GetProperty("sleeve_list");
        Assert.That(sleeveList.ValueKind, Is.EqualTo(JsonValueKind.Object), "wire shape is dict-keyed by series_id string");

        var series = sleeveList.GetProperty("9001");
        Assert.That(series.GetProperty("series_id").GetInt32(), Is.EqualTo(9001));

        var productInfo = series.GetProperty("product_info");
        Assert.That(productInfo.ValueKind, Is.EqualTo(JsonValueKind.Object), "product_info is dict-keyed by product_id string");

        var product = productInfo.GetProperty("900101");
        Assert.That(product.GetProperty("product_id").GetInt32(), Is.EqualTo(900101));
        Assert.That(product.GetProperty("name").GetString(), Is.EqualTo("sleeve_test"));
        Assert.That(product.GetProperty("price_crystal").GetInt32(), Is.EqualTo(400));
        Assert.That(product.GetProperty("is_purchased_product").GetBoolean(), Is.False);
        Assert.That(product.GetProperty("rewards").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task Buy_with_crystals_debits_currency_and_grants_cosmetics()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"product_id":900101,"sales_type":1}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(3));   // crystal post-state + emblem + sleeve

        // First entry: crystal balance post-debit. reward_type=2 (Crystal), reward_id=0, num=600 (1000-400).
        var crystal = rewardList[0];
        Assert.That(crystal.GetProperty("reward_type").GetInt32(), Is.EqualTo(2));
        Assert.That(crystal.GetProperty("reward_id").GetInt64(), Is.EqualTo(0));
        Assert.That(crystal.GetProperty("reward_num").GetInt32(), Is.EqualTo(600));

        // Viewer state: crystals decremented; sleeve + emblem in owned collections.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.Sleeves)
            .Include(v => v.Emblems)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.Crystals, Is.EqualTo(600UL));
        Assert.That(viewer.Sleeves.Any(s => s.Id == 9000011), Is.True);
        Assert.That(viewer.Emblems.Any(e => e.Id == 9000011), Is.True);
    }

    [Test]
    public async Task Buy_with_insufficient_crystals_rejects_with_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 100);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"product_id":900101,"sales_type":1}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Buy_with_series_product_mismatch_rejects_with_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // product 900101 is in series 9001, not 9999
        var response = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9999,"product_id":900101,"sales_type":1}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Buy_already_purchased_sleeve_rejects_with_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        // First buy succeeds
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var first = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"product_id":900101,"sales_type":1}"""));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Second buy rejected
        var second = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"product_id":900101,"sales_type":1}"""));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Buy_ticket_sales_type_returns_501()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/sleeve/buy",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","series_id":9001,"product_id":900101,"sales_type":3}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
    }

    [Test]
    public async Task Info_marks_already_owned_sleeve_as_purchased()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalProduct(factory, viewerId, crystals: 1000);

        // Pre-grant the sleeve so /info should flag is_purchased_product=true
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Sleeves).FirstAsync(v => v.Id == viewerId);
            var sleeve = await db.Sleeves.FindAsync(9000011);
            viewer.Sleeves.Add(sleeve!);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/sleeve/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var product = doc.RootElement
            .GetProperty("sleeve_list").GetProperty("9001")
            .GetProperty("product_info").GetProperty("900101");
        Assert.That(product.GetProperty("is_purchased_product").GetBoolean(), Is.True);
    }
}
