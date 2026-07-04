using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class LoadControllerFreeplayTests
{
    private static StringContent Body() => new(
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam","card_master_hash":""}""",
        Encoding.UTF8, "application/json");

    [Test]
    public async Task LoadIndex_freeplay_on_inflates_currency_and_grants_all_cards()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        // Seed one collectible card so EffectiveOwnedCardsAsync has at least one entry
        // (the minimal test set has no CollectionInfo rows — those cards are non-collectible).
        await factory.SeedOwnedCardAsync(viewerId, 50001001L, count: 1);
        await factory.EnableFreeplayAsync();

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/load/index", Body());
        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Wire key for UserCurrency is "user_crystal_count" (IndexResponse.[JsonPropertyName("user_crystal_count")])
        Assert.That(root.GetProperty("user_crystal_count").GetProperty("crystal").GetUInt64(), Is.EqualTo(99999UL));
        Assert.That(root.GetProperty("user_crystal_count").GetProperty("rupy").GetUInt64(), Is.EqualTo(99999UL));
        Assert.That(root.GetProperty("user_crystal_count").GetProperty("red_ether").GetUInt64(), Is.EqualTo(99999UL));

        var cards = root.GetProperty("user_card_list");
        Assert.That(cards.GetArrayLength(), Is.GreaterThan(0));
        // Wire key for card count is "number" (UserCard.[JsonPropertyName("number")])
        for (int i = 0; i < cards.GetArrayLength(); i++)
            Assert.That(cards[i].GetProperty("number").GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task LoadIndex_freeplay_off_unchanged_baseline()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/load/index", Body());
        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), json);

        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("user_crystal_count").GetProperty("crystal").GetUInt64(),
            Is.Not.EqualTo(99999UL), "freeplay off must not inflate currency");
    }
}
