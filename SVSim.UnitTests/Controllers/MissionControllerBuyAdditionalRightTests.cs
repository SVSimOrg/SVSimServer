using System.Net;
using System.Text;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class MissionControllerBuyAdditionalRightTests
{
    [Test]
    public async Task BuyAdditionalRight_returns_ok()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Body shape is INFERRED — we send the bare base envelope and expect the server
        // to refresh MissionInfoDetail.
        var requestJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/mission/buy_additional_right",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
