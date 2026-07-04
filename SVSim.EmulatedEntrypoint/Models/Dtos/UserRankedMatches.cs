using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserRankedMatches
{
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }
    [JsonPropertyName("match_count")]
    [Key("match_count")]
    public int MatchCount { get; set; }
    [JsonPropertyName("win")]
    [Key("win")]
    public int Wins { get; set; }
    [JsonPropertyName("lose")]
    [Key("lose")]
    public int Losses { get; set; }
    [JsonPropertyName("viewer_id")]
    [Key("viewer_id")]
    public ulong ViewerId { get; set; }
}