using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class RankingControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private const string EmptyBody = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    [Test]
    public async Task GetViewablePeriodList_returns_six_family_arrays()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/get_viewable_ranking_period_list", JsonBody(EmptyBody));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement;

        foreach (var key in new[] { "rank_match", "master_point", "two_pick", "sealed",
                                    "crossover_rank_match", "crossover_master_point" })
        {
            Assert.That(data.TryGetProperty(key, out _), Is.True, $"missing key {key}");
        }
        Assert.That(data.GetProperty("rank_match").GetArrayLength(), Is.GreaterThan(0));
        Assert.That(data.GetProperty("crossover_rank_match").GetArrayLength(), Is.EqualTo(0));
        Assert.That(data.GetProperty("crossover_master_point").GetArrayLength(), Is.EqualTo(0));

        // Master-point entries carry the extra "necessary_score" field per capture.
        var mp0 = data.GetProperty("master_point")[0];
        Assert.That(mp0.GetProperty("necessary_score").GetString(), Is.EqualTo("0"));

        // Two-pick entries carry "type" and "over_460".
        var tp0 = data.GetProperty("two_pick")[0];
        Assert.That(tp0.GetProperty("type").GetString(), Is.EqualTo("2"));
        Assert.That(tp0.GetProperty("over_460").GetString(), Is.EqualTo("1"));
    }

    [Test]
    public async Task MasterPointRotationInfo_returns_empty_ranking_with_period_echoed()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/master_point_rotation_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":1}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("ranking").GetArrayLength(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("period").GetProperty("id").GetString(), Is.EqualTo("1"));
    }

    [Test]
    public async Task MasterPointUnlimitedInfo_returns_empty_ranking()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/master_point_unlimited_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":1}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("ranking").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task RankMatchClassWinRotationInfo_accepts_class_id_and_returns_empty()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/rank_match_class_win_rotation_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":1,"class_id":3}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("ranking").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task RankMatchClassWinUnlimitedInfo_returns_empty()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/rank_match_class_win_unlimited_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":1,"class_id":1}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task TwoPickWinInfo_returns_empty_ranking()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/two_pick_win_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":1}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("ranking").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task MasterPointRotationInfo_unknown_period_returns_empty_period()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_030_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/ranking/master_point_rotation_info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","period_id":99999}"""));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var raw = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("ranking").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Unauthenticated_get_viewable_period_list_returns_401()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();

        var resp = await client.PostAsync("/ranking/get_viewable_ranking_period_list", JsonBody(EmptyBody));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
