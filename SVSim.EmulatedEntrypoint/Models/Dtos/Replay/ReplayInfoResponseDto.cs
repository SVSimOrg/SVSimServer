using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Replay;

[MessagePackObject]
public sealed class ReplayInfoResponseDto
{
    /// <summary>
    /// Required — client does not guard with Keys.Contains. Emit empty list, never null.
    /// </summary>
    [JsonPropertyName("replay_list"), Key("replay_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<ReplayInfoItemDto> ReplayList { get; set; } = new();

    // feature_maintenance_list intentionally omitted — optional per spec, never set
    // because we don't gate replay viewing. WhenWritingNull drops it from the wire.
}
