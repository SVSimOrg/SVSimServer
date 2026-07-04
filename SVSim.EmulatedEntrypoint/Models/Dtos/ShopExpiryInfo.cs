using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class ShopExpiryInfo
{
    [JsonPropertyName("sales_period_time")]
    [Key("sales_period_time")]
    public DateTime? SalesPeriodTime { get; set; }
    [JsonPropertyName("sales_period_series")]
    [Key("sales_period_series")]
    public int? SalesPeriodSeries { get; set; }
}