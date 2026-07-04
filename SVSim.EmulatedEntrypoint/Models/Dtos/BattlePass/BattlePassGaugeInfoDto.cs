using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// gauge_info as emitted on /battle_pass/info (slim shape). The delta-payload fields
/// (point_add, before_*, *_point breakdown, is_premium) are NOT on /info per
/// Wizard/BattlePassGaugeInfo.cs:69 — they appear only when battle_pass_gauge_info
/// is embedded on a battle-finish response. That embedding is out of v1 scope.
/// </summary>
[MessagePackObject]
public class BattlePassGaugeInfoDto
{
    [JsonPropertyName("current_point")]
    [Key("current_point")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string CurrentPoint { get; set; } = "0";

    [JsonPropertyName("current_level")]
    [Key("current_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string CurrentLevel { get; set; } = "1";

    [JsonPropertyName("weekly_battle_pass_point")]
    [Key("weekly_battle_pass_point")]
    public int WeeklyBattlePassPoint { get; set; }

    [JsonPropertyName("weekly_limit_point")]
    [Key("weekly_limit_point")]
    public int WeeklyLimitPoint { get; set; }
}
