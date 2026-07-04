using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Replay;

/// <summary>
/// /replay/detail request. ReplayDetailTaskParam declares <c>new int viewer_id</c>
/// shadowing the inherited field — wire ships both. We accept both; the body's
/// <c>viewer_id</c> identifies the replay owner (typically same as caller).
/// </summary>
[MessagePackObject]
public sealed class ReplayDetailRequestDto : BaseRequest
{
    [JsonPropertyName("battle_id"), Key("battle_id")]
    public long BattleId { get; set; }
}
