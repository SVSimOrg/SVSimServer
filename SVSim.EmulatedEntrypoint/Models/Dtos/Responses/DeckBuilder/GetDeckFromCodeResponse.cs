using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;

[MessagePackObject]
public class GetDeckFromCodeResponse
{
    [JsonPropertyName("text")]
    [Key("text")]
    public string Text { get; set; } = "OK";

    [JsonPropertyName("deck")]
    [Key("deck")]
    public DeckPayload Deck { get; set; } = new();

    [JsonPropertyName("errors")]
    [Key("errors")]
    public PortalErrors Errors { get; set; } = new();
}

/// <summary>
/// Wire shape inside the <c>deck</c> envelope. Prod emits <c>clan</c> / <c>deck_format</c> as
/// strings but <c>sub_clan</c> / <c>rotation_id</c> as ints — mirror that quirk so the client
/// `.ToInt()` / `.ToString()` paths see what they expect. <c>RotationId</c> is typed as
/// <c>object</c> so we can emit the int literal <c>0</c> on standard decks (matches prod) and a
/// string on MyRotation decks.
/// </summary>
[MessagePackObject]
public class DeckPayload
{
    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public string DeckFormat { get; set; } = "1";

    [JsonPropertyName("clan")]
    [Key("clan")]
    public string Clan { get; set; } = "0";

    [JsonPropertyName("sub_clan")]
    [Key("sub_clan")]
    public int SubClan { get; set; }

    [JsonPropertyName("rotation_id")]
    [Key("rotation_id")]
    public object RotationId { get; set; } = 0;

    // Wire key is camelCase mid-word capital to mirror the client's `cardID` parser
    // (Wizard/GetDeckDataFromCodeTask.cs:44 reads `jsonData["cardID"]`).
    [JsonPropertyName("cardID")]
    [Key("cardID")]
    public List<long> CardID { get; set; } = new();
}
