using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class BuildDeckControllerGetPurchaseCountTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task SeedEnabledProduct(SVSimTestFactory f, int productId, int max)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BuildDeckSeries.Add(new BuildDeckSeriesEntry
        {
            Id = 101, OrderIndex = 22, IsEnabled = true, NameKey = "BDSSN_test", IntroKey = "BDSI_test",
        });
        db.BuildDeckProducts.Add(new BuildDeckProductEntry
        {
            Id = productId, SeriesId = 101, IsEnabled = true,
            PurchaseNumMax = max, IntroPriceCrystal = 500,
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Returns_zero_current_and_max_for_unbought_product()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedEnabledProduct(factory, productId: 201, max: 3);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":201}""";
        var response = await client.PostAsync("/build_deck/get_purchase_count", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("purchase_num_current").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("purchase_num_max").GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task Returns_NotFound_for_unknown_product()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","product_id":99999}""";
        var response = await client.PostAsync("/build_deck/get_purchase_count", JsonBody(json));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
