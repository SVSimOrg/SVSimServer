using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// One product in /battle_pass/item_list.products[] (Wizard/BattlePassPurchaseInfoTask.cs:32-43).
/// Numerics on this DTO are numeric on the wire (the client uses .ToInt() and the captured
/// shape isn't string-typed here).
/// </summary>
[MessagePackObject]
public class BattlePassProductDto
{
    [JsonPropertyName("id")]
    [Key("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int Id { get; set; }

    [JsonPropertyName("season_id")]
    [Key("season_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int SeasonId { get; set; }

    [JsonPropertyName("name")]
    [Key("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Name { get; set; } = "";

    [JsonPropertyName("price_crystal")]
    [Key("price_crystal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int PriceCrystal { get; set; }

    [JsonPropertyName("description")]
    [Key("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Description { get; set; } = "";

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassSalesPeriodInfoDto SalesPeriodInfo { get; set; } = new();
}
