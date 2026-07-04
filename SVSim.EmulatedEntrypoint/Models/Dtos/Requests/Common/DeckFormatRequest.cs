using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Common;

/// <summary>
/// Common request shape for endpoints scoped by deck format (`/deck/info`,
/// `/practice/deck_list`, etc.). Spec: common/types.ts.md#deck-format-scoped-requests.
/// </summary>
[MessagePackObject]
public class DeckFormatRequest : BaseRequest
{
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
}
