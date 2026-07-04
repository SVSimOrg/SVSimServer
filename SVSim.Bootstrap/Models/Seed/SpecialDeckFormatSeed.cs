using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class SpecialDeckFormatSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("deck_format")] public string DeckFormat { get; set; } = "";
    [JsonPropertyName("end_time")] public string EndTime { get; set; } = "";
}
