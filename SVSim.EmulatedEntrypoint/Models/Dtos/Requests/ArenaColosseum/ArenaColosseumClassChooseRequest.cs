using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/class_choose</c>. Two mutually-exclusive request shapes per
/// class-choose.md — Normal sends <c>class_id</c>, Chaos sends <c>chaos_id</c>. Both fields
/// are bound on this DTO; the server picks the mode by which is non-zero and rejects when
/// both are present (or neither).
/// </summary>
[MessagePackObject]
public sealed class ArenaColosseumClassChooseRequest : BaseRequest
{
    [JsonPropertyName("class_id")] [Key("class_id")]
    public int ClassId { get; set; }

    /// <summary>Chaos sub-mode replay id. 0 in Normal mode.</summary>
    [JsonPropertyName("chaos_id")] [Key("chaos_id")]
    public int ChaosId { get; set; }
}
