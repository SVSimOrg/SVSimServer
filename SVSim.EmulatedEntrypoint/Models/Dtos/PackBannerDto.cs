using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class PackBannerDto
{
    [JsonPropertyName("banner_name")]
    [Key("banner_name")]
    public string BannerName { get; set; } = string.Empty;

    [JsonPropertyName("dialog_title")]
    [Key("dialog_title")]
    public string DialogTitle { get; set; } = string.Empty;
}
