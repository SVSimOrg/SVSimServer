using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// /battle_pass/info → data.season_info. All numerics are wire-strings (Wizard/BattlePassInfoTask.cs:54-58).
/// can_purchase stays a bool (client uses .ToBoolean()).
/// </summary>
[MessagePackObject]
public class BattlePassSeasonInfoDto
{
    [JsonPropertyName("id")]
    [Key("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Id { get; set; } = "";

    [JsonPropertyName("season_name")]
    [Key("season_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string SeasonName { get; set; } = "";

    [JsonPropertyName("max_level")]
    [Key("max_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string MaxLevel { get; set; } = "";

    [JsonPropertyName("start_date")]
    [Key("start_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("end_date")]
    [Key("end_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("can_purchase")]
    [Key("can_purchase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool CanPurchase { get; set; }
}
