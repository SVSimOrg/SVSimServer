using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.DeckBuilder;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Portal endpoints — deck-code mint (<c>/deck_code</c>) and resolve (<c>/deck</c>). In prod
/// these live on shadowverse-portal.com which speaks plaintext msgpack (no AES); the loader
/// redirects them to this app server via a Harmony prefix on
/// <c>CustomPreference.GetDeckBuilderServerURL</c>. The <see cref="NoWireEncryptionAttribute"/>
/// tells the translation middleware to skip the AES wrapper for both directions.
///
/// Deliberately does not extend <see cref="SVSimController"/>: portal traffic is anonymous and
/// the routes need to live at the bare paths (<c>/deck_code</c>, <c>/deck</c>) rather than
/// under a <c>/deckbuilder/...</c> template.
/// </summary>
[ApiController]
[AllowAnonymous]
[NoWireEncryption]
public class DeckBuilderController : ControllerBase
{
    private readonly IDeckCodeService _codes;

    public DeckBuilderController(IDeckCodeService codes)
    {
        _codes = codes;
    }

    [HttpPost("deck_code")]
    public ActionResult<GenerateDeckCodeResponse> Generate(GenerateDeckCodeRequest req)
    {
        if (req.CardID is null || req.CardID.Count == 0)
        {
            return new GenerateDeckCodeResponse
            {
                Text = "INVALID",
                Errors = new() { Type = "INVALID_DECK", Message = "cardID empty" }
            };
        }

        var payload = new DeckPayload
        {
            DeckFormat = req.DeckFormat.ToString(),
            Clan = req.Clan.ToString(),
            SubClan = req.SubClan ?? 0,
            // Standard decks emit int 0; my-rotation decks emit the rotation id as a string.
            // Mixed wire typing matches prod (data_dumps/captures/traffic_prod_deckcode.ndjson).
            RotationId = (object?)req.RotationId ?? 0,
            // Strip the foil flag (ones digit) — matches prod's normalize-on-encode behaviour
            // observed in the traffic dump (e.g. 703441011 → 703441010).
            CardID = req.CardID.Select(id => id - (id % 10)).ToList()
        };

        string code = _codes.Mint(payload);

        return new GenerateDeckCodeResponse
        {
            Text = "OK",
            DeckCode = code
        };
    }

    [HttpPost("deck")]
    public ActionResult<GetDeckFromCodeResponse> Resolve(GetDeckFromCodeRequest req)
    {
        var payload = _codes.TryResolve(req.DeckCode ?? "");
        if (payload is null)
        {
            return new GetDeckFromCodeResponse
            {
                Text = "EXPIRED",
                Deck = new DeckPayload(),
                Errors = new() { Type = "INVALID_DECK_CODE", Message = "Unknown or expired code" }
            };
        }

        return new GetDeckFromCodeResponse
        {
            Text = "OK",
            Deck = payload
        };
    }
}
