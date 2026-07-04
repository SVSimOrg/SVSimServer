using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckUpdateSleeveRequest : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")] public long SleeveId { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
}
