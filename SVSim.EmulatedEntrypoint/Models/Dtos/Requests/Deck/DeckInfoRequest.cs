using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

/// <summary>
/// /deck/info 窶・standard request is `DeckFormatRequest`. Copy-source overload adds
/// `create_deck_format` (the format the user is creating the new deck IN). Server can
/// ignore create_deck_format and return the standard shape; only matters for the
/// cross-format deck-copy UI flow.
/// </summary>
[MessagePackObject]
public class DeckInfoRequest : DeckFormatRequest
{
    [JsonPropertyName("create_deck_format")]
    [Key("create_deck_format")] public int? CreateDeckFormat { get; set; }
}
