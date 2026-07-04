using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserPromotionMatch
{
    [JsonPropertyName("match_count")]
    [Key("match_count")]
    public int MatchCount { get; set; }
    [JsonPropertyName("battle_result")]
    [Key("battle_result")]
    public int BattleResult { get; set; }
    [JsonPropertyName("win")]
    [Key("win")]
    public int Wins { get; set; }
    [JsonPropertyName("lose")]
    [Key("lose")]
    public int Losses { get; set; }
}