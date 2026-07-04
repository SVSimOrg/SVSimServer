using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class AvatarInfo
{
    [JsonPropertyName("abilities")]
    [Key("abilities")]
    public Dictionary<string, AvatarAbility> Abilities { get; set; } = new Dictionary<string, AvatarAbility>();
    /// <summary>
    /// Prod (2026-05-23) sends an empty array here. Distinct shape from MyRotationInfo.Schedules,
    /// which is a dict {free_battle, gathering}. Entry shape TBD when an active Avatar season is
    /// captured — see <see cref="AvatarSchedule"/>.
    /// </summary>
    [JsonPropertyName("schedules")]
    [Key("schedules")]
    public List<AvatarSchedule> Schedules { get; set; } = new();
}