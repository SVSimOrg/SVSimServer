using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors <c>seeds/my-rotation-abilities.json</c>. <c>data</c> is preserved as raw JSON.</summary>
public sealed class MyRotationAbilitySeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
}
