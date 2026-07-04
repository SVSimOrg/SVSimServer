using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Arena;

/// <summary>
/// Wire shape for /arena/get_challenge_info. Parsed by Wizard/ChallangeHistoryInfoTask.cs:25.
/// All 6 fields below are accessed unconditionally — KeyNotFoundException if any is omitted.
/// Stub values for now; TODO: source from a per-season "challenge history" snapshot when we
/// track viewer's lifetime TK2 stats.
/// </summary>
[MessagePackObject]
public class GetChallengeInfoResponseDto
{
    [JsonPropertyName("challenge_name")] [Key("challenge_name")]
    public string ChallengeName { get; set; } = "";

    /// <summary>Client parses via DateTime.Parse — "yyyy-MM-dd HH:mm:ss" works.</summary>
    [JsonPropertyName("begin_time")] [Key("begin_time")]
    public string BeginTime { get; set; } = "";

    [JsonPropertyName("end_time")] [Key("end_time")]
    public string EndTime { get; set; } = "";

    [JsonPropertyName("two_pick_all_win_count")] [Key("two_pick_all_win_count")]
    public int TwoPickAllWinCount { get; set; }

    [JsonPropertyName("reward_step_info")] [Key("reward_step_info")]
    public RewardStepInfoDto RewardStepInfo { get; set; } = new();
}

[MessagePackObject]
public class RewardStepInfoDto
{
    [JsonPropertyName("max_reward_step")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("max_reward_step")]
    public int MaxRewardStep { get; set; }

    /// <summary>
    /// Wire shape: dict keyed by stringified-int win count → stringified-int reward step
    /// (prod capture: <c>{"5":"5","10":"10","15":"15"}</c>). Client parser at
    /// ChallangeHistoryInfoTask.cs:43 iterates by Count + indexed value access, which works
    /// for both arrays and LitJson object-iteration order — but prod always ships the dict.
    /// </summary>
    [JsonPropertyName("reward_step_list")] [Key("reward_step_list")]
    public Dictionary<string, string> RewardStepList { get; set; } = new();
}
