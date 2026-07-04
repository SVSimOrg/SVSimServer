using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Sleeve;

/// <summary>
/// /sleeve/buy request body. sales_type is ShopCommonUtility.SalesType:
/// 0=free, 1=crystal, 2=rupy, 3=ticket (v1: 3 returns 501, no ticket-priced sleeve captured).
/// </summary>
[MessagePackObject]
public class SleeveBuyRequest : BaseRequest
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }

    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("sales_type")]
    [Key("sales_type")]
    public int SalesType { get; set; }
}
