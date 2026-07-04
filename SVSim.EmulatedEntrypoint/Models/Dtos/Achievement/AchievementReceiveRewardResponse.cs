using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Achievement;

[MessagePackObject(keyAsPropertyName: true)]
public class TotalReceiveCountDto
{
    [Key(0)][JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [Key(1)][JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [Key(2)][JsonPropertyName("reward_count")] public int RewardCount { get; set; }
    [Key(3)][JsonPropertyName("item_type")] public int ItemType { get; set; }
    [Key(4)][JsonPropertyName("is_usable")] public bool IsUsable { get; set; } = true;
}

[MessagePackObject(keyAsPropertyName: true)]
public class RewardGrantDto
{
    [Key(0)][JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [Key(1)][JsonPropertyName("reward_id")] public long RewardId { get; set; }
    [Key(2)][JsonPropertyName("reward_num")] public int RewardNum { get; set; }
}

/// <summary>
/// /achievement/receive_reward response — MissionInfoDataDto + two extras consumed by
/// PlayerStaticData.UpdateHaveUserGoodsNumByJsonData per AchievementReceiveRewardTask.cs:33.
/// </summary>
public sealed class AchievementReceiveRewardResponse : MissionInfoDataDto
{
    [JsonPropertyName("total_receive_count_list")] public List<TotalReceiveCountDto> TotalReceiveCountList { get; set; } = new();
    [JsonPropertyName("reward_list")] public List<RewardGrantDto> RewardList { get; set; } = new();
}
