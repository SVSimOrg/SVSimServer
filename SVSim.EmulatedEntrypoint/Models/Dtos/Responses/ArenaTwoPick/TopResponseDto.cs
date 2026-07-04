using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

[MessagePackObject]
public class TopResponseDto
{
    [JsonPropertyName("entry_info")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)] [Key("entry_info")]
    public EntryInfoDto? EntryInfo { get; set; }

    [JsonPropertyName("battle_results")] [Key("battle_results")]
    public BattleResultsDto? BattleResults { get; set; }

    [JsonPropertyName("class_info")] [Key("class_info")]
    public ClassInfoDto? ClassInfo { get; set; }

    [JsonPropertyName("deck_info")] [Key("deck_info")]
    public DeckInfoDto? DeckInfo { get; set; }

    [JsonPropertyName("leader_skin_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("leader_skin_id")]
    public long? LeaderSkinId { get; set; }
}
