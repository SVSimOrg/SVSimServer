using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class PackControllerLeaderSkinOwnedStatusTests
{
    [Test]
    public async Task GetLeaderSkinOwnedStatus_returns_nine_empty_buckets()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"parent_gacha_id":99047,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/pack/get_leader_skin_owned_status",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        // Per spec: keys "0".."8" inclusive, each an empty object.
        for (int classId = 0; classId <= 8; classId++)
        {
            var bucket = doc.RootElement.GetProperty(classId.ToString());
            Assert.That(bucket.ValueKind, Is.EqualTo(JsonValueKind.Object));
            Assert.That(bucket.EnumerateObject().Count(), Is.EqualTo(0));
        }
    }
}
