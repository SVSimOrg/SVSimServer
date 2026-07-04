using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>Wrapper for one track's reward array: <c>{ "reward": [ ... ] }</c>.</summary>
[MessagePackObject]
public class BattlePassRewardListDto
{
    [JsonPropertyName("reward")]
    [Key("reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<BattlePassRewardDto> Reward { get; set; } = new();
}
