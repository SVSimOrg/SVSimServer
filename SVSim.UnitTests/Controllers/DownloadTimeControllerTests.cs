using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class DownloadTimeControllerTests
{
    [TestCase("/download_time/start")]
    [TestCase("/download_time/end")]
    public async Task Returns_200_with_empty_data_object(string path)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var response = await client.PostAsync(path,
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(doc.RootElement.EnumerateObject().Count(), Is.EqualTo(0),
            "Spec calls for empty `data: {}` — DownloadStartTask's optional image_type stays " +
            "absent, DownloadFinishTask doesn't read data at all.");
    }
}
