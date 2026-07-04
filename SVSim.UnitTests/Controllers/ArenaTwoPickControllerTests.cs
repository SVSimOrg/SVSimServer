using System.Net;
using System.Net.Http.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ArenaTwoPickControllerTests
{
    // Every request DTO inherits BaseRequest; the [ApiController] auto-400 path rejects
    // bodies missing the envelope fields. Spread this into JSON bodies in addition to per-
    // endpoint payload.
    private static readonly object Envelope = new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    [Test]
    public async Task Top_unauthenticated_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();
        var resp = await client.PostAsync("/arena_two_pick/top",
            JsonContent.Create(new { mode = 0, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Top_authed_with_no_run_returns_entry_info_null()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/arena_two_pick/top",
            JsonContent.Create(new { mode = 0, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("\"entry_info\":null", body);
    }
}
