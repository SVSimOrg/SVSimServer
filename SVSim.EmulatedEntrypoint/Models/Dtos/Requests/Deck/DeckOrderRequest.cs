using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckOrderRequest : BaseRequest
{
    [JsonPropertyName("deck_order")]
    [Key("deck_order")] public List<int>? DeckOrder { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
}
