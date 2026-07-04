using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class BattlePassMonthlyMissionSeed
{
    [JsonPropertyName("year")] public int Year { get; set; }
    [JsonPropertyName("month")] public int Month { get; set; }
    [JsonPropertyName("order_num")] public int OrderNum { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [JsonPropertyName("battle_pass_point")] public int BattlePassPoint { get; set; }
    [JsonPropertyName("reward_type")] public int? RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long? RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int? RewardNumber { get; set; }
    [JsonPropertyName("event_type")] public string? EventType { get; set; }
    [JsonPropertyName("event_arg")] public int? EventArg { get; set; }
}
