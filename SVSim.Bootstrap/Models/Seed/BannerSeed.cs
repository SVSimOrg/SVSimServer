using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class BannerSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("image_name")] public string ImageName { get; set; } = "";
    [JsonPropertyName("click")] public string Click { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("change_time")] public int ChangeTime { get; set; }
    [JsonPropertyName("remaining_time")] public int RemainingTime { get; set; }
    [JsonPropertyName("image_paths")] public JsonElement ImagePaths { get; set; }
}
