using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class SetDeckRedisRequest : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("class_id")]
    [Key("class_id")] public int ClassId { get; set; }
}
