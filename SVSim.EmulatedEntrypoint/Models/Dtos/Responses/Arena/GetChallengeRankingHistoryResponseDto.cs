using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Arena;

/// <summary>
/// Wire shape for /arena/get_challenge_ranking_history. Prod returns two empty lists
/// (two_pick + sealed) per the season-26 capture. Populated history is per-viewer + per-season
/// ranking snapshots; not tracked locally yet.
/// </summary>
[MessagePackObject]
public class GetChallengeRankingHistoryResponseDto
{
    [JsonPropertyName("two_pick")] [Key("two_pick")]
    public List<object> TwoPick { get; set; } = new();

    [JsonPropertyName("sealed")] [Key("sealed")]
    public List<object> Sealed { get; set; } = new();
}
