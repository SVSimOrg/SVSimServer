using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Prod sends most numeric-looking fields as STRINGS on the gift endpoints (present_id,
/// reward_type, reward_detail_id, reward_count, condition_number, present_limit_type).
/// item_type is an INT. We mirror the prod shape exactly. See the capture at
/// data_dumps/captures/traffic_event_crate_free_pack.ndjson, /gift/top response (line 18).
/// </summary>
[MessagePackObject]
public class PresentDto
{
    [JsonPropertyName("present_id")]
    [Key("present_id")]
    public string PresentId { get; set; } = string.Empty;

    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public string RewardType { get; set; } = string.Empty;

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public string RewardDetailId { get; set; } = string.Empty;

    [JsonPropertyName("reward_count")]
    [Key("reward_count")]
    public string RewardCount { get; set; } = string.Empty;

    [JsonPropertyName("condition_number")]
    [Key("condition_number")]
    public string ConditionNumber { get; set; } = "0";

    [JsonPropertyName("present_limit_type")]
    [Key("present_limit_type")]
    public string PresentLimitType { get; set; } = "0";

    [JsonPropertyName("reward_limit_time")]
    [Key("reward_limit_time")]
    public int RewardLimitTime { get; set; }

    [JsonPropertyName("create_time")]
    [Key("create_time")]
    public string CreateTime { get; set; } = string.Empty;

    /// <summary>Only present on item/pack-ticket entries (reward_type=4); omit on currency entries.</summary>
    [JsonPropertyName("item_type")]
    [Key("item_type")]
    public int? ItemType { get; set; }

    [JsonPropertyName("message")]
    [Key("message")]
    public string Message { get; set; } = string.Empty;
}
