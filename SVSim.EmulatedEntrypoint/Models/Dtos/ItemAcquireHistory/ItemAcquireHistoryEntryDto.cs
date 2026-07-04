using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ItemAcquireHistory;

/// <summary>
/// One row in the <c>/item_acquire_history/info</c> response. All numeric fields ship as
/// decimal strings to match the prod capture.
/// </summary>
[MessagePackObject]
public sealed class ItemAcquireHistoryEntryDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public string RewardType { get; set; } = "0";

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public string RewardDetailId { get; set; } = "0";

    [JsonPropertyName("reward_count")]
    [Key("reward_count")]
    public string RewardCount { get; set; } = "0";

    [JsonPropertyName("acquire_type")]
    [Key("acquire_type")]
    public string AcquireType { get; set; } = "0";

    [JsonPropertyName("acquire_time")]
    [Key("acquire_time")]
    public string AcquireTime { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    [Key("message")]
    public string Message { get; set; } = string.Empty;
}
