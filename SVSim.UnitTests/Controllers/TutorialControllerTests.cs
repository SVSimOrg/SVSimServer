using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class TutorialControllerTests
{
    [Test]
    public async Task UpdateAction_returns_result_code_1_with_empty_data()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // tutorial_step and tutorial_action_number are fire-and-forget bookkeeping fields;
        // send representative values from the live capture (step=1, action=2).
        var requestJson =
            """{"tutorial_step":1,"tutorial_action_number":2,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var response = await client.PostAsync("/tutorial/update_action",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadAsStringAsync();

        // Controllers return the INNER data payload; envelope is middleware's job.
        // For the no-op shape the action returns an empty object.
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(doc.RootElement.EnumerateObject().Count(), Is.EqualTo(0),
            "update_action returns empty data — client uses SkipAllNetworkChecks and reads nothing.");
    }

    [TestCase(11)]
    [TestCase(21)]
    [TestCase(31)]
    public async Task Update_echoes_requested_step_and_persists(int step)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = $$"""
        {"tutorial_step":{{step}},"is_skip":0,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}
        """;

        var response = await client.PostAsync("/tutorial/update",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(step));

        // Side effect: viewer state advanced.
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(step));
    }

    [Test]
    public async Task Update_with_is_skip_1_jumps_to_100()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // The client sends the step it's MOVING TO. is_skip=1 means "skip the rest" — typically
        // sent with tutorial_step=100 already (matches what `TutorialUpdateTask` does with the
        // is_skip flag), so the server's job is just to honor whatever value is provided.
        var requestJson = """{"tutorial_step":100,"is_skip":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var response = await client.PostAsync("/tutorial/update",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(100));
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(100));
    }

    [Test]
    public async Task Update_does_not_regress_step()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Stale/replayed request: client thinks state is still 11 and sends an update for it.
        var requestJson = """{"tutorial_step":11,"is_skip":0,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/tutorial/update",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(11),
            "Response echoes the requested step (the client confirms its own transition).");

        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(100),
            "Persisted state must NOT regress. Math.Max(current, requested) — mirrors the " +
            "31→41 max-preserve pattern in GiftController.TutorialGiftReceive.");
    }
}
