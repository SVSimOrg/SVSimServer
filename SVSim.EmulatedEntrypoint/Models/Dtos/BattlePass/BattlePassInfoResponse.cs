using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// Top-level /battle_pass/info response. premium_appeal_level is optional
/// (Wizard/BattlePassInfoTask.cs:77 — guarded by Keys.Contains).
/// </summary>
[MessagePackObject]
public class BattlePassInfoResponse
{
    [JsonPropertyName("season_info")]
    [Key("season_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassSeasonInfoDto SeasonInfo { get; set; } = new();

    [JsonPropertyName("reward_info")]
    [Key("reward_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassRewardInfoDto RewardInfo { get; set; } = new();

    [JsonPropertyName("gauge_info")]
    [Key("gauge_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassGaugeInfoDto GaugeInfo { get; set; } = new();

    [JsonPropertyName("premium_appeal_level")]
    [Key("premium_appeal_level")]
    public List<int>? PremiumAppealLevel { get; set; }
}
