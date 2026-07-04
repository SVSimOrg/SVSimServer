using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors a single entry in <c>seeds/battle-pass-seasons.json</c>.</summary>
public sealed class BattlePassSeasonSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("max_level")] public int MaxLevel { get; set; }
    [JsonPropertyName("start_date")] public string StartDate { get; set; } = "";
    [JsonPropertyName("end_date")] public string EndDate { get; set; } = "";
    [JsonPropertyName("can_purchase")] public bool CanPurchase { get; set; }
    [JsonPropertyName("price_crystal")] public int PriceCrystal { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}
