using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class CardControllerCreateFoilCardTests
{
    private const int OrbItemId = 1000;

    private static async Task<(long baseCardId, long foilCardId)> SeedFoilPairAsync(SVSimTestFactory factory)
    {
        long baseId = await factory.SeedCardAsync(isFoil: false);
        // SeedCardAsync increments by 2 to keep base/foil at consecutive ids; the foil twin
        // for our baseId is at baseId + 1.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        long foilId = baseId + 1;
        if (!await db.Cards.AnyAsync(c => c.Id == foilId))
        {
            db.Cards.Add(new ShadowverseCardEntry { Id = foilId, IsFoil = true, Name = $"SeedCard{foilId}" });
            await db.SaveChangesAsync();
        }
        return (baseId, foilId);
    }

    private static async Task GiveBaseCopyAsync(SVSimTestFactory factory, long viewerId, long baseCardId, int count)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var card = await db.Cards.FirstAsync(c => c.Id == baseCardId);
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        viewer.Cards.Add(new OwnedCardEntry { Card = card, Count = count, IsProtected = false });
        await db.SaveChangesAsync();
    }

    private static async Task GiveOrbsAsync(SVSimTestFactory factory, long viewerId, int count)
        => await factory.SeedOwnedItemAsync(viewerId, OrbItemId, count, itemName: "Orb", itemType: 1);

    [Test]
    public async Task CreateFoilCard_consumes_one_orb_and_one_normal_copy_and_grants_one_foil()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        var (baseCardId, foilCardId) = await SeedFoilPairAsync(factory);
        await GiveBaseCopyAsync(factory, viewerId, baseCardId, count: 1);
        await GiveOrbsAsync(factory, viewerId, count: 1);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = $$"""{"base_card_id":{{baseCardId}},"base_card_number":1,"create_number":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/card/create_foil_card",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer2 = await db2.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .FirstAsync(v => v.Id == viewerId);

        var foilOwn = viewer2.Cards.SingleOrDefault(c => c.Card.Id == foilCardId);
        var baseOwn = viewer2.Cards.SingleOrDefault(c => c.Card.Id == baseCardId);
        Assert.That(foilOwn?.Count ?? 0, Is.EqualTo(1), "foil count");
        Assert.That(baseOwn?.Count ?? 0, Is.EqualTo(0), "base count after consume");

        var orbBalance = viewer2.Items.SingleOrDefault(i => i.Item.Id == OrbItemId)?.Count ?? 0;
        Assert.That(orbBalance, Is.EqualTo(0), "orb balance after spend");

        using var doc = JsonDocument.Parse(body);
        var rewards = doc.RootElement.GetProperty("reward_list").EnumerateArray().ToList();
        Assert.That(rewards, Is.Not.Empty);
    }

    [Test]
    public async Task CreateFoilCard_rejects_without_orb()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        var (baseCardId, foilCardId) = await SeedFoilPairAsync(factory);
        await GiveBaseCopyAsync(factory, viewerId, baseCardId, count: 1);
        // No orbs granted.

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = $$"""{"base_card_id":{{baseCardId}},"base_card_number":1,"create_number":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/card/create_foil_card",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.Single(c => c.Card.Id == baseCardId).Count, Is.EqualTo(1), "base count untouched");
        Assert.That(viewer.Cards.Any(c => c.Card.Id == foilCardId && c.Count > 0), Is.False, "no foil granted");
    }

    [Test]
    public async Task CreateFoilCard_rejects_without_base_copy()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        var (baseCardId, _) = await SeedFoilPairAsync(factory);
        await GiveOrbsAsync(factory, viewerId, count: 1);
        // No base copy granted.

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = $$"""{"base_card_id":{{baseCardId}},"base_card_number":1,"create_number":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/card/create_foil_card",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Items).ThenInclude(i => i.Item).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Items.Single(i => i.Item.Id == OrbItemId).Count, Is.EqualTo(1), "orb balance untouched");
    }
}
