using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.DeckBuilder;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// End-to-end coverage for the portal pair (/deck_code, /deck). These tests bypass the
/// translation middleware (non-Unity UA) and hit the controllers via plain JSON, which is fine
/// — both endpoints are anonymous and the action signatures don't care which path serialized
/// the body. The middleware's [NoWireEncryption] branch is exercised in the live smoke test.
/// </summary>
public class DeckBuilderControllerTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Test]
    public async Task Generate_then_resolve_roundtrips_deck_payload()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var generate = await client.PostAsJsonAsync("/deck_code",
            new GenerateDeckCodeRequest
            {
                Clan = 4,
                DeckFormat = 1,
                CardID = new() { 100414020, 100414020, 104021030 }
            }, Json);

        Assert.That(generate.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await generate.Content.ReadAsStringAsync());
        var generateBody = await generate.Content.ReadFromJsonAsync<GenerateDeckCodeResponse>(Json);
        Assert.That(generateBody, Is.Not.Null);
        Assert.That(generateBody!.DeckCode, Has.Length.EqualTo(4));

        var resolve = await client.PostAsJsonAsync("/deck",
            new GetDeckFromCodeRequest { DeckCode = generateBody.DeckCode }, Json);
        Assert.That(resolve.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await resolve.Content.ReadAsStringAsync());

        var resolveBody = await resolve.Content.ReadFromJsonAsync<GetDeckFromCodeResponse>(Json);
        Assert.That(resolveBody, Is.Not.Null);
        Assert.That(resolveBody!.Deck.Clan, Is.EqualTo("4"));
        Assert.That(resolveBody.Deck.DeckFormat, Is.EqualTo("1"));
        Assert.That(resolveBody.Deck.SubClan, Is.EqualTo(0));
        Assert.That(resolveBody.Deck.CardID, Is.EqualTo(new List<long> { 100414020, 100414020, 104021030 }));
    }

    [Test]
    public async Task Generate_strips_foil_flag_from_card_ids()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var generate = await client.PostAsJsonAsync("/deck_code",
            new GenerateDeckCodeRequest
            {
                Clan = 4,
                DeckFormat = 1,
                // 011 ids are foil variants observed in the prod traffic dump.
                CardID = new() { 703441011, 701441011, 100414020 }
            }, Json);
        var generateBody = await generate.Content.ReadFromJsonAsync<GenerateDeckCodeResponse>(Json);

        var resolve = await client.PostAsJsonAsync("/deck",
            new GetDeckFromCodeRequest { DeckCode = generateBody!.DeckCode }, Json);
        var resolveBody = await resolve.Content.ReadFromJsonAsync<GetDeckFromCodeResponse>(Json);

        Assert.That(resolveBody!.Deck.CardID,
            Is.EqualTo(new List<long> { 703441010, 701441010, 100414020 }),
            "Foil bit (last digit) must be normalized to 0 in the stored payload.");
    }

    [Test]
    public async Task Resolve_returns_invalid_code_error_for_unknown_code()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var resolve = await client.PostAsJsonAsync("/deck",
            new GetDeckFromCodeRequest { DeckCode = "zzzz" }, Json);
        Assert.That(resolve.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resolve.Content.ReadFromJsonAsync<GetDeckFromCodeResponse>(Json);
        Assert.That(body!.Errors.Type, Is.EqualTo("INVALID_DECK_CODE"));
    }

    [Test]
    public async Task Generate_rejects_empty_card_list()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var generate = await client.PostAsJsonAsync("/deck_code",
            new GenerateDeckCodeRequest { Clan = 1, DeckFormat = 1, CardID = new() }, Json);
        var body = await generate.Content.ReadFromJsonAsync<GenerateDeckCodeResponse>(Json);

        Assert.That(body!.Errors.Type, Is.EqualTo("INVALID_DECK"));
        Assert.That(body.DeckCode, Is.Empty);
    }
}
