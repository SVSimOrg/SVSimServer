using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.DeckBuilder;

/// <summary>
/// Covers all three client-side overloads of <c>GenerateDeckCodeTask.SetParameter</c>:
/// standard, crossover (sub_clan present), and my-rotation (rotation_id present, no phantom).
/// Optional fields stay null on shapes that don't carry them.
///
/// Deliberately does NOT inherit from <see cref="BaseRequest"/>: portal endpoints are anonymous
/// (the server ignores viewer_id / steam_id / steam_session_ticket on the wire — see the
/// data_headers in the prod traffic dump where they're all zeroed). The fields still arrive on
/// the wire from the client; System.Text.Json silently drops unknown JSON properties.
/// </summary>
[MessagePackObject]
public class GenerateDeckCodeRequest
{
    [JsonPropertyName("clan")]
    [Key("clan")]
    public int Clan { get; set; }

    [JsonPropertyName("sub_clan")]
    [Key("sub_clan")]
    public int? SubClan { get; set; }

    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public int DeckFormat { get; set; }

    // Wire key is camelCase mid-word capital — verified in data_dumps/captures/traffic.ndjson live
    // capture (`"cardID":[...]`). The client's LitJson serializer emits the C# property name
    // verbatim, and the param classes in Wizard/GenerateDeckCodeTask.cs use `cardID` /
    // `phantomCardID`. Snake-case would silently bind to empty and the controller would emit
    // INVALID_DECK; that was the 2026-05-28 "blank code in the deck builder UI" symptom.
    [JsonPropertyName("cardID")]
    [Key("cardID")]
    public List<long> CardID { get; set; } = new();

    [JsonPropertyName("phantomCardID")]
    [Key("phantomCardID")]
    public List<long>? PhantomCardID { get; set; }

    [JsonPropertyName("rotation_id")]
    [Key("rotation_id")]
    public string? RotationId { get; set; }
}
