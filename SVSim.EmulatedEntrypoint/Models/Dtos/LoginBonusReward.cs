using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One day's reward in a login-bonus cycle. All numeric fields ship as quoted strings on
/// the wire — prod-confirmed shape from traffic_prod_tutorial.ndjson. Client uses
/// JsonData.ToInt() so it tolerates either form, but we match prod exactly.
/// </summary>
[MessagePackObject]
public class LoginBonusReward
{
    [JsonPropertyName("effect_id")] [Key("effect_id")]
    public string EffectId { get; set; } = "0";

    [JsonPropertyName("reward_type")] [Key("reward_type")]
    public string RewardType { get; set; } = "0";

    [JsonPropertyName("reward_detail_id")] [Key("reward_detail_id")]
    public string RewardDetailId { get; set; } = "0";

    [JsonPropertyName("reward_number")] [Key("reward_number")]
    public string RewardNumber { get; set; } = "0";
}
