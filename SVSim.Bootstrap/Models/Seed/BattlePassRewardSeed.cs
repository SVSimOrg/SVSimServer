using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors a single entry in <c>seeds/battle-pass-rewards.json</c>.</summary>
public sealed class BattlePassRewardSeed
{
    [JsonPropertyName("season_id")] public int SeasonId { get; set; }
    [JsonPropertyName("track")] public string Track { get; set; } = "";       // "normal" / "premium"
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("is_appeal_exclusion")] public bool IsAppealExclusion { get; set; }
}
