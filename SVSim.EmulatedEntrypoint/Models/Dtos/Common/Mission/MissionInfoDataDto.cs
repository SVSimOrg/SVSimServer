using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

/// <summary>
/// Top-level payload for /mission/info responses (also reused by /mission/retire,
/// /mission/change_receive_setting; /achievement/receive_reward adds reward_list +
/// total_receive_count_list to this shape via inheritance).
///
/// CanChangeMissionTime is wire-required to be present (capture shows null when active).
/// Override [JsonIgnore(Condition = Never)] per memory project_wire_null_policy.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class MissionInfoDataDto
{
    [Key(0)][JsonPropertyName("user_mission_list")] public List<UserMissionDto> UserMissionList { get; set; } = new();
    [Key(1)][JsonPropertyName("is_change_mission")] public bool IsChangeMission { get; set; }

    [Key(2)]
    [JsonPropertyName("can_change_mission_time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long? CanChangeMissionTime { get; set; }

    [Key(3)][JsonPropertyName("is_change_receive_type")] public bool IsChangeReceiveType { get; set; }

    [Key(4)]
    [JsonPropertyName("can_change_receive_type_time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long? CanChangeReceiveTypeTime { get; set; }

    [Key(5)][JsonPropertyName("user_achievement_list")] public List<UserAchievementDto> UserAchievementList { get; set; } = new();
    [Key(6)][JsonPropertyName("mission_receive_type")] public string MissionReceiveType { get; set; } = "0";

    [Key(7)]
    [JsonPropertyName("battle_pass_monthly_mission")]
    public BPMonthlyMissionsDto? BattlePassMonthlyMission { get; set; }
}
