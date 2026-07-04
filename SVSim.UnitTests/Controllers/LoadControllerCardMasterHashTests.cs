using System.Net;
using System.Text;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Covers the response side of card-master freshness: server emits inner
/// <c>data.card_master_hash</c> on <c>/load/index</c> only when the request's hash differs
/// from <c>CardMasterConfig.CurrentHash</c>. Presence-only client check + emit-every-time
/// would force a 1.27 MB redownload on every boot.
/// </summary>
public class LoadControllerCardMasterHashTests
{
    private const string PinnedHash = "94b5c44edc51ff76c0af8fcc894af12f979dd38c:1";

    private static string IndexRequestJsonWithHash(string hash) =>
        $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam","card_master_hash":"{{hash}}"}""";

    [Test]
    public async Task Index_omits_card_master_hash_when_request_matches_server_hash()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJsonWithHash(PinnedHash), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("card_master_hash", out _), Is.False,
            "Expected card_master_hash OMITTED when request matches server. Body: " + body);
    }

    [Test]
    public async Task Index_emits_card_master_hash_when_request_differs()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJsonWithHash("oldhash:1"), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("card_master_hash", out var hashEl), Is.True,
            "Expected card_master_hash PRESENT when request differs. Body: " + body);
        Assert.That(hashEl.GetString(), Is.EqualTo(PinnedHash));
    }

    [Test]
    public async Task Index_emits_card_master_hash_when_request_hash_empty()
    {
        // Empty hash = fresh client with no cardmaster/card_master_1 on disk
        // (CardMasterLocalFileUtility.GetCardMasterHash returns ""). Treat as mismatch.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJsonWithHash(""), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("card_master_hash", out var hashEl), Is.True,
            "Expected card_master_hash PRESENT for fresh client. Body: " + body);
        Assert.That(hashEl.GetString(), Is.EqualTo(PinnedHash));
    }
}
