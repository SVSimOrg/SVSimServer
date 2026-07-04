using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BuildDeck;

/// <summary>
/// /build_deck/buy request body. sales_type is ShopCommonUtility.SalesType:
/// 0=free, 1=crystal, 2=rupy, 3=ticket (v1: 3 returns 501).
/// </summary>
[MessagePackObject]
public class BuildDeckBuyRequest : BaseRequest
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("sales_type")]
    [Key("sales_type")]
    public int SalesType { get; set; }
}
