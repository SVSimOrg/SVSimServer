using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BuildDeck;

[MessagePackObject]
public class BuildDeckGetPurchaseCountRequest : BaseRequest
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }
}
