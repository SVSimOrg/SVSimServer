using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>reward_info: { normal: { reward: [...] }, premium: { reward: [...] } }.</summary>
[MessagePackObject]
public class BattlePassRewardInfoDto
{
    [JsonPropertyName("normal")]
    [Key("normal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassRewardListDto Normal { get; set; } = new();

    [JsonPropertyName("premium")]
    [Key("premium")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public BattlePassRewardListDto Premium { get; set; } = new();
}
