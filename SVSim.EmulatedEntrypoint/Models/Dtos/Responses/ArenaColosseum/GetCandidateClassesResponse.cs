using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/get_candidate_classes</c>. Two mutually-exclusive sub-shapes —
/// Normal 2-pick emits <c>class_id_1/2/3</c>; Chaos emits <c>chaos_id_1/2/3</c> +
/// <c>chaos_info</c>. <c>WhenWritingNull</c> strips the inactive branch so the wire matches
/// the spec exactly.
/// </summary>
[MessagePackObject]
public sealed class GetCandidateClassesResponse
{
    [JsonPropertyName("class_id_1")] [Key("class_id_1")]
    public int? ClassId1 { get; set; }

    [JsonPropertyName("class_id_2")] [Key("class_id_2")]
    public int? ClassId2 { get; set; }

    [JsonPropertyName("class_id_3")] [Key("class_id_3")]
    public int? ClassId3 { get; set; }

    [JsonPropertyName("chaos_id_1")] [Key("chaos_id_1")]
    public int? ChaosId1 { get; set; }

    [JsonPropertyName("chaos_id_2")] [Key("chaos_id_2")]
    public int? ChaosId2 { get; set; }

    [JsonPropertyName("chaos_id_3")] [Key("chaos_id_3")]
    public int? ChaosId3 { get; set; }

    [JsonPropertyName("selected_chaos_id")] [Key("selected_chaos_id")]
    public int? SelectedChaosId { get; set; }

    [JsonPropertyName("selected_class_id")] [Key("selected_class_id")]
    public int? SelectedClassId { get; set; }

    [JsonPropertyName("selected_leader_skin_id")] [Key("selected_leader_skin_id")]
    public long? SelectedLeaderSkinId { get; set; }
}
