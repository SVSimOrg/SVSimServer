using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry from /mypage/index data.banner — the home-screen promo carousel. Consumed by
/// MyPageBannerBase.BannerInfo.Parse(jsonData[i]) when the client iterates banner[i] (banner
/// access is TryGetValue-guarded but the per-entry parse is unconditional).
///
/// Prod-captured shape:
/// <code>
/// {"image_name":"banner_000788","click":"account_transition_with_two","status":"10",
///  "change_time":"10","remaining_time":"0","image_paths":[]}
/// </code>
///
/// Note: change_time, remaining_time, and status are strings on the wire (PHP convention) even
/// though they look numeric. The DB stores them in matching column types but the wire shape rules.
/// </summary>
[MessagePackObject]
public class BannerInfo
{
    [JsonPropertyName("image_name")]
    [Key("image_name")]
    public string ImageName { get; set; } = string.Empty;

    [JsonPropertyName("click")]
    [Key("click")]
    public string Click { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Key("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("change_time")]
    [Key("change_time")]
    public string ChangeTime { get; set; } = string.Empty;

    [JsonPropertyName("remaining_time")]
    [Key("remaining_time")]
    public string RemainingTime { get; set; } = string.Empty;

    [JsonPropertyName("image_paths")]
    [Key("image_paths")]
    public List<string> ImagePaths { get; set; } = new();
}
