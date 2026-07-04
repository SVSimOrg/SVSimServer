using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Pack;

[MessagePackObject]
public class GetGachaPointRewardsResponse
{
    [JsonPropertyName("gacha_point_rewards")]
    [Key("gacha_point_rewards")]
    public List<GachaPointRewardDto> GachaPointRewards { get; set; } = new();
}
