using System.Net;
using System.Text;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class EmblemControllerFavoriteTests
{
    [Test]
    public async Task Favorite_accepts_add_and_remove_lists()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"favorite_add":["100","101"],"favorite_remove":["200"],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/emblem/favorite",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Favorite_accepts_empty_lists()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"favorite_add":[],"favorite_remove":[],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/emblem/favorite",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
