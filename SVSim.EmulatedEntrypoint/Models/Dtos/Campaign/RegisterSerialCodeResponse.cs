using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Campaign;

/// <summary>
/// Success response shape. Failure path uses an anonymous <c>{ result_code = 4202 }</c>
/// (mirroring AchievementController/MissionController) and bypasses this DTO.
/// </summary>
[MessagePackObject]
public sealed class RegisterSerialCodeResponse
{
    [JsonPropertyName("is_complete")]
    [Key("is_complete")]
    public bool IsComplete { get; set; }
}
