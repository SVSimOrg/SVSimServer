using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckUpdateRandomLeaderSkinRequest : BaseRequest
{
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")] public List<int>? LeaderSkinIdList { get; set; }
}
