using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class BattlePassControllerItemListTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");
    private const string EmptyAuthBody = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private static async Task SeedSeason23(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.BattlePassSeasons.Add(new BattlePassSeasonEntry
        {
            Id = 23, Name = "Season 23", MaxLevel = 100,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CanPurchase = true, PriceCrystal = 980,
            Description = "Unlock premium track.",
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task ItemList_returns_one_product_for_active_season()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/item_list", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        using var doc = JsonDocument.Parse(body);
        var products = doc.RootElement.GetProperty("products");
        Assert.That(products.GetArrayLength(), Is.EqualTo(1));
        var product = products[0];
        Assert.That(product.GetProperty("id").GetInt32(), Is.EqualTo(23000)); // 23 * 1000
        Assert.That(product.GetProperty("season_id").GetInt32(), Is.EqualTo(23));
        Assert.That(product.GetProperty("price_crystal").GetInt32(), Is.EqualTo(980));
    }

    [Test]
    public async Task ItemList_returns_empty_products_when_viewer_already_premium()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedSeason23(factory);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattlePassProgress.Add(new ViewerBattlePassProgressEntry
            {
                ViewerId = viewerId, SeasonId = 23, CurrentPoint = 0,
                IsPremium = true, WeeklyPoints = 0,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/battle_pass/item_list", JsonBody(EmptyAuthBody));
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("products").GetArrayLength(), Is.EqualTo(0));
    }
}
