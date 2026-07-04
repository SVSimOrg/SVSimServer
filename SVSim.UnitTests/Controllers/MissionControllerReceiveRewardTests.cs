using System.Net;
using System.Text;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class MissionControllerReceiveRewardTests
{
    [Test]
    public async Task ReceiveReward_returns_ok()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // INFERRED: id of the mission to claim. Defensive stub accepts even when
        // the id doesn't match a current mission — there is no decomp Parse() to
        // tell us how the server errors.
        var requestJson = """{"id":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/mission/receive_reward",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
