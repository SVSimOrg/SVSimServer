using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PackDrawConfigSeed
{
    [JsonPropertyName("pack_id")] public int PackId { get; set; }
    [JsonPropertyName("short_code")] public string? ShortCode { get; set; }
    [JsonPropertyName("animation_rate_pct")] public double AnimationRatePct { get; set; }
    [JsonPropertyName("has_bonus_slot")] public bool HasBonusSlot { get; set; }
    [JsonPropertyName("special_kind")] public string? SpecialKind { get; set; }
}
