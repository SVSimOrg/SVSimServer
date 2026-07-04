using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors a single entry in <c>seeds/battle-pass-levels.json</c>.</summary>
public sealed class BattlePassLevelSeed
{
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("required_point")] public int RequiredPoint { get; set; }
}
