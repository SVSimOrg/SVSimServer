using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Nested under <c>colosseum_info.sales_period_info</c>. Captured prod shape — single
/// <c>sales_period_time</c> field carrying the wall-clock end of the cup's sales window.
/// Format <c>"yyyy-MM-dd HH:mm:ss"</c> (PHP convention, not ISO).
/// </summary>
[MessagePackObject]
public sealed class ColosseumSalesPeriodInfo
{
    [JsonPropertyName("sales_period_time")] [Key("sales_period_time")]
    public string SalesPeriodTime { get; set; } = string.Empty;
}
