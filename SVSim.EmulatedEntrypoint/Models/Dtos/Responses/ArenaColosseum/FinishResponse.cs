using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/finish</c> and <c>POST /arena_colosseum/retire</c>. Wire-identical
/// shape per finish.md §"Wire-shared with /retire" — endpoints differ in side-effects only:
/// /finish emits per-round-completion + champion bonus and marks the entry champion;
/// /retire emits the round-capped consolation. The client renders <c>rewards</c> in the
/// post-bracket popup, applies wallet deltas via <c>reward_list</c>, and updates the
/// status block via <c>colosseum_status</c>.
/// </summary>
[MessagePackObject]
public sealed class FinishResponse
{
    [JsonPropertyName("rewards")] [Key("rewards")]
    public List<ColosseumReceivedReward> Rewards { get; set; } = new();

    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<RewardEntryDto> RewardList { get; set; } = new();

    [JsonPropertyName("colosseum_status")] [Key("colosseum_status")]
    public ColosseumOwnStatus ColosseumStatus { get; set; } = new();
}
