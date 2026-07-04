using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class AchievementCatalogSeed
{
    [JsonPropertyName("achievement_type")] public int AchievementType { get; set; }
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("order_num")] public int OrderNum { get; set; }
    [JsonPropertyName("event_type")] public string? EventType { get; set; }
    [JsonPropertyName("event_arg")] public int? EventArg { get; set; }
}
