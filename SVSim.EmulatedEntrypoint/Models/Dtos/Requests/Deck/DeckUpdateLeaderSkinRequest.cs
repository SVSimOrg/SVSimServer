using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckUpdateLeaderSkinRequest : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
}
