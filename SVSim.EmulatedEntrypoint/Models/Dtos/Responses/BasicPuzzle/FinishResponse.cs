using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.BasicPuzzle;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BasicPuzzle;

[MessagePackObject]
public class FinishResponse
{
    [JsonPropertyName("add_point")] [Key("add_point")]
    public int? AddPoint { get; set; } = null;

    /// <summary>STRING "1" on wins, NUMBER 0 on losses — both observed in prod. Per-call wire type
    /// quirk; controller writes the right one based on is_win.</summary>
    [JsonPropertyName("win_count")] [Key("win_count")]
    public object WinCount { get; set; } = 0;

    [JsonPropertyName("class_experience")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_experience")]
    public int ClassExperience { get; set; } = 0;

    [JsonPropertyName("class_level")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_level")]
    public int ClassLevel { get; set; } = 1;

    [JsonPropertyName("achieved_info")] [Key("achieved_info")]
    public AchievedInfoResponse AchievedInfo { get; set; } = new();

    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<TreasureRewardResponse> RewardList { get; set; } = new();

    [JsonPropertyName("class_bonus_point")] [Key("class_bonus_point")]
    public int ClassBonusPoint { get; set; } = 0;

    [JsonPropertyName("format_bonus_point")] [Key("format_bonus_point")]
    public int FormatBonusPoint { get; set; } = 0;

    [JsonPropertyName("required_win_count_for_win_bonus_point")] [Key("required_win_count_for_win_bonus_point")]
    public int RequiredWinCountForWinBonusPoint { get; set; } = 0;

    [JsonPropertyName("win_bonus_point")] [Key("win_bonus_point")]
    public int WinBonusPoint { get; set; } = 0;

    [JsonPropertyName("win_bonus_point_status")] [Key("win_bonus_point_status")]
    public int WinBonusPointStatus { get; set; } = 0;

    [JsonPropertyName("get_class_experience")] [Key("get_class_experience")]
    public int GetClassExperience { get; set; } = 0;

    [JsonPropertyName("clear_mission_list")] [Key("clear_mission_list")]
    public ClearMissionListResponse ClearMissionList { get; set; } = new();

    [JsonPropertyName("spot_point_data")] [Key("spot_point_data")]
    public SpotPointDataResponse SpotPointData { get; set; } = new();

    [JsonPropertyName("puzzle_list")] [Key("puzzle_list")]
    public List<PuzzleGroupResponse> PuzzleList { get; set; } = new();
}

[MessagePackObject]
public class AchievedInfoResponse
{
    [JsonPropertyName("achieved_mission_list")] [Key("achieved_mission_list")]
    public List<PuzzleAchievedMissionEntry> AchievedMissionList { get; set; } = new();

    [JsonPropertyName("achieved_mission_reward_list")] [Key("achieved_mission_reward_list")]
    public List<PuzzleAchievedMissionReward> AchievedMissionRewardList { get; set; } = new();

    [JsonPropertyName("mission_start_data")] [Key("mission_start_data")]
    public List<MissionStartEntry> MissionStartData { get; set; } = new();

    [JsonPropertyName("battle_pass_reward_list")] [Key("battle_pass_reward_list")]
    public List<object> BattlePassRewardList { get; set; } = new();

    [JsonPropertyName("battle_pass_message_list")] [Key("battle_pass_message_list")]
    public List<object> BattlePassMessageList { get; set; } = new();
}

[MessagePackObject]
public class PuzzleAchievedMissionEntry
{
    [JsonPropertyName("achieved_message")] [Key("achieved_message")]
    public string AchievedMessage { get; set; } = string.Empty;
}

[MessagePackObject]
public class PuzzleAchievedMissionReward
{
    [JsonPropertyName("mission_reward_type")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("mission_reward_type")]
    public int MissionRewardType { get; set; }

    [JsonPropertyName("mission_reward_detail_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("mission_reward_detail_id")]
    public long MissionRewardDetailId { get; set; }

    [JsonPropertyName("mission_reward_number")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("mission_reward_number")]
    public int MissionRewardNumber { get; set; }
}

[MessagePackObject]
public class MissionStartEntry
{
    [JsonPropertyName("mission_name")] [Key("mission_name")]
    public string MissionName { get; set; } = string.Empty;

    [JsonPropertyName("start_time")] [Key("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("lot_type")] [Key("lot_type")]
    public string LotType { get; set; } = "3"; // Phase 1 only emits puzzle-mission lot_type
}

[MessagePackObject]
public class TreasureRewardResponse
{
    [JsonPropertyName("reward_type")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("reward_id")]
    public long RewardId { get; set; }

    [JsonPropertyName("reward_num")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_num")]
    public int RewardNum { get; set; }
}

[MessagePackObject]
public class ClearMissionListResponse
{
    [JsonPropertyName("common_mission")] [Key("common_mission")]
    public List<object> CommonMission { get; set; } = new();

    [JsonPropertyName("character_mission")] [Key("character_mission")]
    public List<object> CharacterMission { get; set; } = new();
}

[MessagePackObject]
public class SpotPointDataResponse
{
    [JsonPropertyName("before_spot_point")] [Key("before_spot_point")] public int BeforeSpotPoint { get; set; }
    [JsonPropertyName("add_spot_point")] [Key("add_spot_point")] public int AddSpotPoint { get; set; }
    [JsonPropertyName("after_spot_point")] [Key("after_spot_point")] public int AfterSpotPoint { get; set; }
}
