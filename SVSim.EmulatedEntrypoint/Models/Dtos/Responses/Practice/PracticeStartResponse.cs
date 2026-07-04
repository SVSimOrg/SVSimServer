using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Practice;

[MessagePackObject]
public class PracticeStartResponse
{
    /// <summary>
    /// Mission/achievement evaluation snapshot. Client reads it via
    /// `data.Keys.Contains("mission_parameter")` so omitting the key is technically
    /// safe — but prod always emits `mission_parameter: []` and matching prod exactly
    /// avoids surprises if any other code path drops the defensive check.
    /// </summary>
    [JsonPropertyName("mission_parameter")]
    [Key("mission_parameter")] public List<object> MissionParameter { get; set; } = new();
}
