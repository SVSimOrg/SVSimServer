using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckDeleteListRequest : BaseRequest
{
    [JsonPropertyName("deck_no_list")]
    [Key("deck_no_list")] public List<int>? DeckNoList { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
}
