using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class AccountControllerTests
{
    [Test]
    public async Task UpdateName_writes_display_name()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"name":"littlefootse","viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var response = await client.PostAsync("/account/update_name",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify persisted name.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.DisplayName, Is.EqualTo("littlefootse"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t\n")]
    public async Task UpdateName_rejects_empty_or_whitespace(string name)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = $$"""{"name":{{JsonSerializer.Serialize(name)}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/account/update_name",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Display name remains the seeded default ("Test Viewer").
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.DisplayName, Is.EqualTo("Test Viewer"));
    }

    [Test]
    public async Task UpdateName_rejects_explicit_null()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Explicit JSON null — used to NRE when the controller assigned request.Name
        // (default string.Empty) straight to viewer.DisplayName without a null check.
        var requestJson = """{"name":null,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/account/update_name",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateName_rejects_too_long_name()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // 25 chars > the 24-char server cap.
        var name = new string('a', 25);
        var requestJson = $$"""{"name":"{{name}}","viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/account/update_name",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateName_accepts_name_at_cap_boundary()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Exactly 24 chars — boundary case.
        var name = new string('a', 24);
        var requestJson = $$"""{"name":"{{name}}","viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/account/update_name",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.DisplayName, Is.EqualTo(name));
    }

    [Test]
    public async Task UpdateBirth_persists_and_round_trips_through_load_index()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = """{"birth":"1995-07-04","viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var setResp = await client.PostAsync("/account/update_birth",
            new StringContent(setJson, Encoding.UTF8, "application/json"));
        var setBody = await setResp.Content.ReadAsStringAsync();
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), setBody);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Info.BirthDate.ToString("yyyy-MM-dd"), Is.EqualTo("1995-07-04"));

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));
        var body = await loadResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("user_info").GetProperty("birth").GetString(),
            Is.EqualTo("1995-07-04"), body);
    }

    [TestCase("not-a-date")]
    [TestCase("1995/07/04")]
    [TestCase("1995-7-4")]
    [TestCase("")]
    public async Task UpdateBirth_rejects_malformed_input(string birth)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"birth":{{JsonSerializer.Serialize(birth)}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/account/update_birth",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
