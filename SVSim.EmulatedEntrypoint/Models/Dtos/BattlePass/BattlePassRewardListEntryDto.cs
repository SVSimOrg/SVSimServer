using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// One entry in /battle_pass/buy → reward_list[]. POST-STATE TOTALS for affected goods +
/// currency, per memory project_wire_reward_list_post_state. Matches RewardListEntry shape
/// used by /pack/open etc.
/// </summary>
[MessagePackObject]
public class BattlePassRewardListEntryDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_id")]
    [Key("reward_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long RewardId { get; set; }

    [JsonPropertyName("reward_num")]
    [Key("reward_num")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int RewardNum { get; set; }
}
