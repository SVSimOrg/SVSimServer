using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors a single entry in <c>seeds/mission-catalog.json</c>.</summary>
public sealed class MissionCatalogSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("lot_type")] public int LotType { get; set; }
    [JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("battle_pass_point")] public int BattlePassPoint { get; set; }
    [JsonPropertyName("default_flag")] public bool DefaultFlag { get; set; }
    [JsonPropertyName("event_type")] public string? EventType { get; set; }
    [JsonPropertyName("event_arg")] public int? EventArg { get; set; }
    [JsonPropertyName("start_time")] public long StartTime { get; set; }
    [JsonPropertyName("end_time")] public long? EndTime { get; set; }
}
