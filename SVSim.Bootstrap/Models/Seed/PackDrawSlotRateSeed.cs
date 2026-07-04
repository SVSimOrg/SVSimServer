using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PackDrawSlotRateSeed
{
    [JsonPropertyName("pack_id")] public int PackId { get; set; }
    [JsonPropertyName("slot")] public string Slot { get; set; } = "general";
    [JsonPropertyName("tier")] public string Tier { get; set; } = "bronze";
    [JsonPropertyName("rate_pct")] public double RatePct { get; set; }
}
