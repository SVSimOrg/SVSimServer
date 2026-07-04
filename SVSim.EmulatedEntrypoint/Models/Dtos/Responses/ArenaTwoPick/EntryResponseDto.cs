using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

[MessagePackObject]
public class EntryResponseDto
{
    [JsonPropertyName("entry_info")] [Key("entry_info")]
    public EntryInfoDto EntryInfo { get; set; } = new();

    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<RewardEntryDto> RewardList { get; set; } = new();

    [JsonPropertyName("candidate_class_ids")] [Key("candidate_class_ids")]
    public List<int> CandidateClassIds { get; set; } = new();

    [JsonPropertyName("battle_results")] [Key("battle_results")]
    public BattleResultsDto BattleResults { get; set; } = new();
}
