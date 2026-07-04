using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Profile;

[MessagePackObject]
public sealed class ProfileIndexResponse
{
    [JsonPropertyName("user_rank_match_total_win")]
    [Key("user_rank_match_total_win")]
    public int UserRankMatchTotalWin { get; set; }

    [JsonPropertyName("user_class_list")]
    [Key("user_class_list")]
    public List<UserClass> UserClassList { get; set; } = new();
}
