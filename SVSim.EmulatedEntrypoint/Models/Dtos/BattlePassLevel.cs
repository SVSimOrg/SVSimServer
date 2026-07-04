using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry under <c>/load/index.battle_pass_level_info</c>. Per memory
/// project_wire_key_serialization, wire ints are strings here — client parses them via .ToInt().
/// </summary>
[MessagePackObject]
public class BattlePassLevel
{
    [JsonPropertyName("level")]
    [Key("level")]
    public string Level { get; set; } = "";

    [JsonPropertyName("required_point")]
    [Key("required_point")]
    public string RequiredPoint { get; set; } = "";
}
