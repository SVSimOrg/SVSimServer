using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BasicPuzzle;

[MessagePackObject]
public class PuzzleMissionResponse
{
    [JsonPropertyName("mission_name")] [Key("mission_name")]
    public string MissionName { get; set; } = string.Empty;

    [JsonPropertyName("require_number")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("require_number")]
    public int RequireNumber { get; set; }

    [JsonPropertyName("campaign_commence_time")] [Key("campaign_commence_time")]
    public long CampaignCommenceTime { get; set; }

    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<PuzzleMissionRewardResponse> RewardList { get; set; } = new();

    [JsonPropertyName("order_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("total_count")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("is_achieved")] [Key("is_achieved")]
    public bool IsAchieved { get; set; }
}

[MessagePackObject]
public class PuzzleMissionRewardResponse
{
    [JsonPropertyName("reward_type")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_number")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_number")]
    public int RewardNumber { get; set; }
}
