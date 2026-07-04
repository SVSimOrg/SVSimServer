using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Practice;

[MessagePackObject]
public class PracticeFinishResponse
{
    /// <summary>Class XP gained this match.</summary>
    [JsonPropertyName("get_class_experience")]
    [Key("get_class_experience")] public int GetClassExperience { get; set; }

    /// <summary>Total accumulated class XP for the played class after this match.</summary>
    [JsonPropertyName("class_experience")]
    [Key("class_experience")] public int ClassExperience { get; set; }

    /// <summary>Class level after this match (post-promotion if XP rolled over).</summary>
    [JsonPropertyName("class_level")]
    [Key("class_level")] public int ClassLevel { get; set; } = 1;

    /// <summary>
    /// Missions / achievements / rewards rollup. Empty dict means "nothing accumulated"
    /// (spec: parser tolerates empty object).
    /// </summary>
    [JsonPropertyName("achieved_info")]
    [Key("achieved_info")] public Dictionary<string, object> AchievedInfo { get; set; } = new();

    /// <summary>Standard reward grants applied to user's inventory. Empty by default.</summary>
    [JsonPropertyName("reward_list")]
    [Key("reward_list")] public List<Reward> RewardList { get; set; } = new();
}
