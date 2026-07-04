using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// /battle_pass/buy response (Wizard/BattlePassBuyTask.cs:30-52). result_code carries the
/// envelope failure path: 22 = insufficient crystals, 23 = already premium,
/// 24 = outside BP period / season mismatch.
/// </summary>
[MessagePackObject]
public class BattlePassBuyResponse
{
    [JsonPropertyName("result_code")]
    [Key("result_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int ResultCode { get; set; } = 1;

    [JsonPropertyName("achieved_info")]
    [Key("achieved_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassAchievedInfoDto AchievedInfo { get; set; } = new();

    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<BattlePassRewardListEntryDto> RewardList { get; set; } = new();
}
