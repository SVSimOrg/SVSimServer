using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// One entry in /battle_pass/buy → achieved_info.battle_pass_reward_list[]
/// (Wizard/BattlePassBuyTask.cs:42-48). Delta entries; numerics are int here, not string-typed.
/// </summary>
[MessagePackObject]
public class BattlePassReceivedRewardDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int RewardNumber { get; set; }
}
