using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/get_candidate_cards</c>. Idempotent draft-resume — server emits
/// the current snapshot for the active run plus the pending pair offer. The Common
/// <c>DeckInfoDto</c>/<c>CandidatePairDto</c>/<c>ClassInfoDto</c> shapes are shared with
/// arena-two-pick and arena-competition per spec.
/// </summary>
[MessagePackObject]
public sealed class GetCandidateCardsResponse
{
    [JsonPropertyName("deck_info")] [Key("deck_info")]
    public DeckInfoDto DeckInfo { get; set; } = new();

    [JsonPropertyName("candidate_card_list")] [Key("candidate_card_list")]
    public List<CandidatePairDto> CandidateCardList { get; set; } = new();

    [JsonPropertyName("leader_skin_id")] [Key("leader_skin_id")]
    public long? LeaderSkinId { get; set; }

    [JsonPropertyName("class_info")] [Key("class_info")]
    public ClassInfoDto ClassInfo { get; set; } = new();
}
