using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

[MessagePackObject]
public class SpecialTitleCheckResponse
{
    /// <summary>
    /// Numeric string. "0"/"1" are the built-in default title screens; any other value
    /// is treated as an asset-bundle id. When omitted, the client defaults to "0".
    /// </summary>
    [JsonPropertyName("title_image_id")]
    [Key("title_image_id")]
    public string? TitleImageId { get; set; }
}
