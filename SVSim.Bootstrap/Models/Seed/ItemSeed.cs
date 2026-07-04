using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class ItemSeed
{
    [JsonPropertyName("item_id")] public int ItemId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("thumbnail_path")] public string ThumbnailPath { get; set; } = "";
}
