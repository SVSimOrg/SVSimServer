using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PuzzleMissionSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("mission_name")] public string MissionName { get; set; } = "";
    [JsonPropertyName("achieved_message")] public string AchievedMessage { get; set; } = "";
    [JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [JsonPropertyName("campaign_commence_time")] public long CampaignCommenceTime { get; set; }
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("target_puzzle_group_id")] public int? TargetPuzzleGroupId { get; set; }
}
