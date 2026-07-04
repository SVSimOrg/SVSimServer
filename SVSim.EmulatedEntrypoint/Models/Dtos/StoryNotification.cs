using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// story_notification on /mypage/index, consumed by
/// StoryNotification.SetStoryNotification (Wizard/StoryNotification.cs:20). The
/// outer key is directly indexed; the inner fields are TryGetValue-defaulted.
/// </summary>
[MessagePackObject]
public class StoryNotification
{
    [JsonPropertyName("is_display_ribbon")]
    [Key("is_display_ribbon")]
    public bool IsDisplayRibbon { get; set; }

    [JsonPropertyName("is_display_badge")]
    [Key("is_display_badge")]
    public bool IsDisplayBadge { get; set; }
}
