using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

/// <summary>
/// Wire shape of UserAchievement (per MissionInfoDetail.cs:98-116). ios/android are always
/// empty strings in our world. max_level is computed from catalog (MAX(Level) per type).
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class UserAchievementDto
{
    [Key(0)][JsonPropertyName("achievement_type")] public int AchievementType { get; set; }
    [Key(1)][JsonPropertyName("achievement_status")] public int AchievementStatus { get; set; }
    [Key(2)][JsonPropertyName("level")] public int Level { get; set; }
    [Key(3)][JsonPropertyName("now_achieved_level")] public int NowAchievedLevel { get; set; }
    [Key(4)][JsonPropertyName("result_announce_saw_level")] public int ResultAnnounceSawLevel { get; set; }
    [Key(5)][JsonPropertyName("total_count")] public int TotalCount { get; set; }
    [Key(6)][JsonPropertyName("achievement_name")] public string AchievementName { get; set; } = "";
    [Key(7)][JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [Key(8)][JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [Key(9)][JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [Key(10)][JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [Key(11)][JsonPropertyName("max_level")] public int MaxLevel { get; set; }
    [Key(12)][JsonPropertyName("order_num")] public int OrderNum { get; set; }
    [Key(13)][JsonPropertyName("ios")] public string Ios { get; set; } = "";
    [Key(14)][JsonPropertyName("android")] public string Android { get; set; } = "";
}
