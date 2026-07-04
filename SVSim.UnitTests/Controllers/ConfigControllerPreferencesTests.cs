using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ConfigControllerPreferencesTests
{
    [Test]
    public async Task UpdateFoilPreferred_persists_and_appears_in_load_index()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = """{"is_foil_preferred":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var setResp = await client.PostAsync("/config/update_foil_preferred",
            new StringContent(setJson, Encoding.UTF8, "application/json"));
        var setBody = await setResp.Content.ReadAsStringAsync();
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), setBody);

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));
        var body = await loadResp.Content.ReadAsStringAsync();
        Assert.That(loadResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var cfg = doc.RootElement.GetProperty("user_config");
        Assert.That(cfg.GetProperty("is_foil_preferred").GetInt32(), Is.EqualTo(1), body);
    }

    [Test]
    public async Task UpdatePrizePreferred_persists_and_appears_in_load_index()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = """{"is_prize_preferred":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var setResp = await client.PostAsync("/config/update_prize_preferred",
            new StringContent(setJson, Encoding.UTF8, "application/json"));
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));

        var body = await loadResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var cfg = doc.RootElement.GetProperty("user_config");
        Assert.That(cfg.GetProperty("is_prize_preferred").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task Update_persists_is_skip_gacha_effect_and_appears_in_load_index()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = """{"is_skip_gacha_effect":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var setResp = await client.PostAsync("/config/update",
            new StringContent(setJson, Encoding.UTF8, "application/json"));
        var setBody = await setResp.Content.ReadAsStringAsync();
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), setBody);

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));
        var body = await loadResp.Content.ReadAsStringAsync();
        Assert.That(loadResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var cfg = doc.RootElement.GetProperty("user_config");
        Assert.That(cfg.GetProperty("is_skip_gacha_effect").GetInt32(), Is.EqualTo(1), body);
    }

    [Test]
    public async Task UpdateChallengeConfig_round_trips_through_load_index()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = """{"use_challenge_two_pick_premium_card":1,"challenge_two_pick_sleeve_id":3000099,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var setResp = await client.PostAsync("/config/update_challenge_config",
            new StringContent(setJson, Encoding.UTF8, "application/json"));
        var setBody = await setResp.Content.ReadAsStringAsync();
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), setBody);

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));
        var body = await loadResp.Content.ReadAsStringAsync();
        Assert.That(loadResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var challenge = doc.RootElement.GetProperty("challenge_config");
        Assert.That(challenge.GetProperty("use_challenge_two_pick_premium_card").GetInt32(), Is.EqualTo(1), body);
        Assert.That(challenge.GetProperty("challenge_two_pick_sleeve_id").GetInt32(), Is.EqualTo(3000099), body);
    }

    [Test]
    public async Task LoadIndex_challenge_config_falls_back_to_default_sleeve_when_viewer_unset()
    {
        // Fresh viewer never touched /config/update_challenge_config — premium card defaults
        // to 0 and sleeve falls back to DefaultLoadoutConfig.SleeveId (3000011).
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var loadJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"none","card_master_hash":""}""";
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(loadJson, Encoding.UTF8, "application/json"));
        var body = await loadResp.Content.ReadAsStringAsync();
        Assert.That(loadResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var challenge = doc.RootElement.GetProperty("challenge_config");
        Assert.That(challenge.GetProperty("use_challenge_two_pick_premium_card").GetInt32(), Is.EqualTo(0), body);
        Assert.That(challenge.GetProperty("challenge_two_pick_sleeve_id").GetInt32(), Is.EqualTo(3000011), body);
    }
}
