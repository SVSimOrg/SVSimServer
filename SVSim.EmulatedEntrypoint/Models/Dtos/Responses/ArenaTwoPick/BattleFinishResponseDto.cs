using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

[MessagePackObject]
public class BattleFinishResponseDto
{
    [JsonPropertyName("class_experience")] [Key("class_experience")]
    public int ClassExperience { get; set; }

    [JsonPropertyName("get_class_experience")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("get_class_experience")]
    public int GetClassExperience { get; set; }

    [JsonPropertyName("class_level")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_level")]
    public int ClassLevel { get; set; }

    [JsonPropertyName("battle_result")] [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("spot_point_info")] [Key("spot_point_info")]
    public SpotPointInfoDto SpotPointInfo { get; set; } = new();

    [JsonPropertyName("achieved_info")] [Key("achieved_info")]
    public ArenaTwoPickAchievedInfoDto AchievedInfo { get; set; } = new();

    [JsonPropertyName("result_decision")] [Key("result_decision")]
    public int ResultDecision { get; set; } = 1;

    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<RewardEntryDto> RewardList { get; set; } = new();

    [JsonPropertyName("gathering_notification")] [Key("gathering_notification")]
    public GatheringNotificationDto GatheringNotification { get; set; } = new();

    [JsonPropertyName("battle_dialog_list")] [Key("battle_dialog_list")]
    public List<object> BattleDialogList { get; set; } = new();
}

[MessagePackObject]
public class SpotPointInfoDto
{
    [JsonPropertyName("before_spot_point")] [Key("before_spot_point")] public int BeforeSpotPoint { get; set; }
    [JsonPropertyName("add_spot_point")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("add_spot_point")] public int AddSpotPoint { get; set; }
    [JsonPropertyName("after_spot_point")] [Key("after_spot_point")] public int AfterSpotPoint { get; set; }
}

[MessagePackObject]
public class GatheringNotificationDto
{
    [JsonPropertyName("matching_established_message")] [Key("matching_established_message")]
    public string MatchingEstablishedMessage { get; set; } = "";
}

[MessagePackObject]
public class ArenaTwoPickAchievedInfoDto
{
    [JsonPropertyName("achieved_achievement_list")] [Key("achieved_achievement_list")]
    public List<object> AchievedAchievementList { get; set; } = new();

    [JsonPropertyName("achieved_mission_list")] [Key("achieved_mission_list")]
    public List<object> AchievedMissionList { get; set; } = new();

    [JsonPropertyName("achieved_mission_reward_list")] [Key("achieved_mission_reward_list")]
    public List<object> AchievedMissionRewardList { get; set; } = new();

    [JsonPropertyName("grand_master_reward_list")] [Key("grand_master_reward_list")]
    public List<object> GrandMasterRewardList { get; set; } = new();

    [JsonPropertyName("mission_start_data")] [Key("mission_start_data")]
    public List<object> MissionStartData { get; set; } = new();
}
