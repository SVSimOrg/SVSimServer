using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class BuildDeckControllerInfoTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task SeedTwoSeries(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var seriesA = new BuildDeckSeriesEntry
        {
            Id = 101, OrderIndex = 22, IsEnabled = true, IsNew = false,
            NameKey = "BDSSN_A", IntroKey = "BDSI_A",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 1, SeriesId = 101, LeaderId = 1, DeckCode = "pd0101",
                    ProductNameKey = "BDPN_A_elf", FeaturedCardId = 100,
                    PurchaseNumMax = 3, IntroPriceCrystal = 500, RegularPriceCrystal = 750,
                    IsEnabled = true,
                },
            },
        };
        var seriesB = new BuildDeckSeriesEntry
        {
            Id = 107, OrderIndex = 15, IsEnabled = true, IsNew = false,
            NameKey = "BDSSN_B", IntroKey = "BDSI_B",
            Products =
            {
                new BuildDeckProductEntry
                {
                    Id = 701, SeriesId = 107, LeaderId = 1, DeckCode = "pd0107",
                    ProductNameKey = "BDPN_B_elf", FeaturedCardId = 200,
                    PurchaseNumMax = 1, IntroPriceCrystal = 1200,
                    IsEnabled = true,
                },
            },
        };
        var disabled = new BuildDeckSeriesEntry
        {
            Id = 10100, OrderIndex = 999, IsEnabled = false, NameKey = "BDSSN_TEMP", IntroKey = "BDSI_TEMP",
        };
        db.BuildDeckSeries.AddRange(seriesA, seriesB, disabled);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Returns_only_enabled_series_sorted_by_order_index_desc()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedTwoSeries(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","add_series_id":0}""";
        var response = await client.PostAsync("/build_deck/info", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var list = doc.RootElement;   // controller returns a bare array — `data` IS the series list
        Assert.That(list.GetArrayLength(), Is.EqualTo(2));
        Assert.That(list[0].GetProperty("series_id").GetInt32(), Is.EqualTo(101), "OrderIndex 22 sorts first");
        Assert.That(list[1].GetProperty("series_id").GetInt32(), Is.EqualTo(107));
    }

    [Test]
    public async Task Filters_to_single_series_when_add_series_id_nonzero()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedTwoSeries(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","add_series_id":107}""";
        var response = await client.PostAsync("/build_deck/info", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var list = doc.RootElement;
        Assert.That(list.GetArrayLength(), Is.EqualTo(1));
        Assert.That(list[0].GetProperty("series_id").GetInt32(), Is.EqualTo(107));
    }

    [Test]
    public async Task Emits_intro_price_and_is_first_price_true_for_unbought_max3_product()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedTwoSeries(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","add_series_id":101}""";
        var response = await client.PostAsync("/build_deck/info", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var product = doc.RootElement[0].GetProperty("products")[0];
        Assert.That(product.GetProperty("is_first_price").GetBoolean(), Is.True);
        Assert.That(product.GetProperty("price_crystal").GetInt32(), Is.EqualTo(500));
        Assert.That(product.GetProperty("purchase_num_current").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public async Task Emits_regular_price_after_first_purchase_recorded()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedTwoSeries(factory);

        // Record a purchase directly to simulate post-buy state.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.BuildDeckPurchases).FirstAsync(x => x.Id == viewerId);
            v.BuildDeckPurchases.Add(new ViewerBuildDeckProductPurchase { ProductId = 1, PurchaseCount = 1 });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","add_series_id":101}""";
        var response = await client.PostAsync("/build_deck/info", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var product = doc.RootElement[0].GetProperty("products")[0];
        Assert.That(product.GetProperty("is_first_price").GetBoolean(), Is.False);
        Assert.That(product.GetProperty("price_crystal").GetInt32(), Is.EqualTo(750));
        Assert.That(product.GetProperty("purchase_num_current").GetInt32(), Is.EqualTo(1));
    }
}
