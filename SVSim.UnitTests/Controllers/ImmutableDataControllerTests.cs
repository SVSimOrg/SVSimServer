using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ImmutableDataControllerTests
{
    private const string CardMasterRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_master_hash":""}""";

    [Test]
    public async Task CardMaster_returns_200_with_base64_payload()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/immutable_data/card_master",
            new StringContent(CardMasterRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.TryGetProperty("card_master", out var blob), Is.True,
            "Response missing card_master field. Body: " + body);
        Assert.That(blob.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(blob.GetString()!.Length, Is.GreaterThan(1_000_000),
            "Expected ~1.27 MB base64 payload; got " + blob.GetString()!.Length);
    }

    [Test]
    public async Task CardMaster_payload_decodes_to_slot1_csv_with_expected_row_count()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/immutable_data/card_master",
            new StringContent(CardMasterRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var b64 = doc.RootElement.GetProperty("card_master").GetString()!;

        var gzipped = Convert.FromBase64String(b64);
        using var ms = new MemoryStream(gzipped);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var sr = new StreamReader(gz, Encoding.UTF8);
        var json = sr.ReadToEnd();

        using var inner = JsonDocument.Parse(json);
        Assert.That(inner.RootElement.TryGetProperty("1", out var slot1), Is.True,
            "Decoded blob missing slot '1' (Default CardMasterId)");
        var csv = slot1.GetString()!;
        var rowCount = csv.Split('\n').Length;
        Assert.That(rowCount, Is.EqualTo(11867),
            "Expected 11867 CSV rows from the 2026-06-03 captured blob; got " + rowCount);
        Assert.That(csv.Split('\n')[0], Does.StartWith("930844060,930844061,90000,CN_930844060,0,930844060,"),
            "Row 0 doesn't match the captured prod row — blob may be corrupted or swapped.");
    }
}
