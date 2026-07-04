using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PackDrawCardWeightSeed
{
    [JsonPropertyName("pack_id")] public int PackId { get; set; }
    [JsonPropertyName("slot")] public string Slot { get; set; } = "general";
    [JsonPropertyName("tier")] public string Tier { get; set; } = "bronze";
    [JsonPropertyName("class_id")] public int? ClassId { get; set; }
    [JsonPropertyName("card_id")] public long CardId { get; set; }
    [JsonPropertyName("rate_pct")] public double? RatePct { get; set; }
    [JsonPropertyName("is_leader")] public bool IsLeader { get; set; }
    [JsonPropertyName("is_alt_art")] public bool IsAltArt { get; set; }
}
