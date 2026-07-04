using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/feature-maintenances.json</c>. Source: <c>/load/index
/// data.feature_maintenance_list</c> (array of dicts; usually empty). <see cref="Data"/> is
/// the raw element so it round-trips verbatim into the entity's jsonb column.
/// </summary>
public sealed class FeatureMaintenanceSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("feature_key")] public string FeatureKey { get; set; } = "";
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
}
