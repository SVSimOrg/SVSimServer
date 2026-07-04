using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class ArenaConfig
{
    [JsonPropertyName("use_challenge_two_pick_premium_card")]
    [Key("use_challenge_two_pick_premium_card")]
    public int UseChallengePickTwoPremiumCard { get; set; }
    [JsonPropertyName("challenge_two_pick_sleeve_id")]
    [Key("challenge_two_pick_sleeve_id")]
    public int ChallengePickTwoCardSleeve { get; set; }
}