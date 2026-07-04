using System.Net;
using System.Text;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class AccountControllerUpdateRegionCodeTests
{
    [TestCase(0)]
    [TestCase(1)]
    public async Task UpdateRegionCode_accepts(int initializeFlag)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = $$"""{"initialize_flag":{{initializeFlag}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/account/update_region_code",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
