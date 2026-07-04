using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class ConfigUpdateChallengeConfigRequest : BaseRequest
{
    [JsonPropertyName("use_challenge_two_pick_premium_card")]
    [Key("use_challenge_two_pick_premium_card")]
    public int UseChallengeTwoPickPremiumCard { get; set; }

    [JsonPropertyName("challenge_two_pick_sleeve_id")]
    [Key("challenge_two_pick_sleeve_id")]
    public long ChallengeTwoPickSleeveId { get; set; }
}
