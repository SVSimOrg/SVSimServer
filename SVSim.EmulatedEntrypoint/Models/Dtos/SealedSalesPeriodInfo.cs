using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Nested under /mypage/index data.sealed_info.sales_period_info. Distinct from Arena/Colosseum's
/// sales_period_info shapes — this inner value is an int (the active schedule series number),
/// not a date string. Captured from prod: <c>"sales_period_info": { "sales_period_series": 33 }</c>.
/// </summary>
[MessagePackObject]
public class SealedSalesPeriodInfo
{
    [JsonPropertyName("sales_period_series")]
    [Key("sales_period_series")]
    public int SalesPeriodSeries { get; set; }
}
