using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class EntryRequest : BaseRequest
{
    [JsonPropertyName("consume_item_type")] [Key("consume_item_type")] public int ConsumeItemType { get; set; }
}
