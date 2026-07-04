using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

[MessagePackObject]
public class CardChooseResponseDto
{
    [JsonPropertyName("deck_info")] [Key("deck_info")]
    public DeckInfoDto DeckInfo { get; set; } = new();

    [JsonPropertyName("candidate_card_list")] [Key("candidate_card_list")]
    public List<CandidatePairDto>? CandidateCardList { get; set; }
}
