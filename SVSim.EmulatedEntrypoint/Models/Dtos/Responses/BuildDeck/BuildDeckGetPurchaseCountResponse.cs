using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BuildDeck;

[MessagePackObject]
public class BuildDeckGetPurchaseCountResponse
{
    [JsonPropertyName("purchase_num_current")]
    [Key("purchase_num_current")]
    public int PurchaseNumCurrent { get; set; }

    [JsonPropertyName("purchase_num_max")]
    [Key("purchase_num_max")]
    public int PurchaseNumMax { get; set; }
}
