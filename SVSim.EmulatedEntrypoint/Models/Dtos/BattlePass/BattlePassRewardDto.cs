using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// One reward inside reward_info.normal.reward[] or reward_info.premium.reward[]
/// (Wizard/BattlePassReward.cs:23-32). Numerics are wire-strings; is_received is bool.
/// is_appeal_exclusion ("0"/"1") is omitted from normal track and present on premium.
/// </summary>
[MessagePackObject]
public class BattlePassRewardDto
{
    [JsonPropertyName("reward_level")]
    [Key("reward_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string RewardLevel { get; set; } = "";

    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string RewardType { get; set; } = "";

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string RewardDetailId { get; set; } = "";

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string RewardNumber { get; set; } = "";

    [JsonPropertyName("is_received")]
    [Key("is_received")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool IsReceived { get; set; }

    /// <summary>"0" or "1"; null/omitted on normal track.</summary>
    [JsonPropertyName("is_appeal_exclusion")]
    [Key("is_appeal_exclusion")]
    public string? IsAppealExclusion { get; set; }
}
