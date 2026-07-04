using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// achieved_info wrapper inside /battle_pass/buy response
/// (Wizard/BattlePassBuyTask.cs:37-40).
/// </summary>
[MessagePackObject]
public class BattlePassAchievedInfoDto
{
    [JsonPropertyName("battle_pass_reward_list")]
    [Key("battle_pass_reward_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<BattlePassReceivedRewardDto> BattlePassRewardList { get; set; } = new();
}
