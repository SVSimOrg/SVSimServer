using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// sales_period_info on /battle_pass/item_list (Wizard/BattlePassPurchaseInfoTask.cs:26-29).
/// Only sales_period_time is read by the client; other fields are unused but allowed.
/// </summary>
[MessagePackObject]
public class BattlePassSalesPeriodInfoDto
{
    [JsonPropertyName("sales_period_time")]
    [Key("sales_period_time")]
    public string? SalesPeriodTime { get; set; }
}
