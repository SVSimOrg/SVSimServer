using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// /battle_pass/item_list response (Wizard/BattlePassPurchaseInfoTask.cs:23-44).
/// </summary>
[MessagePackObject]
public class BattlePassItemListResponse
{
    [JsonPropertyName("premium_pass_description")]
    [Key("premium_pass_description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string PremiumPassDescription { get; set; } = "";

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassSalesPeriodInfoDto SalesPeriodInfo { get; set; } = new();

    [JsonPropertyName("products")]
    [Key("products")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<BattlePassProductDto> Products { get; set; } = new();
}
