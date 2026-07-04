using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// "quest" block on /mypage/index. Consumed by QuestOpenInfo.SetOpenInfo.
/// Empty/closed-quest shape captured from prod 2026-05-23.
/// </summary>
[MessagePackObject]
public class Quest
{
    [JsonPropertyName("is_open")]
    [Key("is_open")]
    public bool IsOpen { get; set; }

    [JsonPropertyName("is_display_badge")]
    [Key("is_display_badge")]
    public bool IsDisplayBadge { get; set; }

    [JsonPropertyName("is_daily_first_access")]
    [Key("is_daily_first_access")]
    public bool IsDailyFirstAccess { get; set; }

    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = string.Empty;
}
