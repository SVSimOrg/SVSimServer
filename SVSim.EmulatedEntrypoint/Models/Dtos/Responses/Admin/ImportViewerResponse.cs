using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Admin;

public class ImportViewerResponse
{
    [JsonPropertyName("viewer_id")] public long ViewerId { get; set; }
    [JsonPropertyName("short_udid")] public long ShortUdid { get; set; }
    [JsonPropertyName("was_created")] public bool WasCreated { get; set; }
    [JsonPropertyName("skipped_card_count")] public int SkippedCardCount { get; set; }
    [JsonPropertyName("skipped_mission_count")] public int SkippedMissionCount { get; set; }
    [JsonPropertyName("skipped_mission_counter_count")] public int SkippedMissionCounterCount { get; set; }
    [JsonPropertyName("skipped_achievement_count")] public int SkippedAchievementCount { get; set; }
    [JsonPropertyName("skipped_achievement_counter_count")] public int SkippedAchievementCounterCount { get; set; }
    [JsonPropertyName("skipped_story_count")] public int SkippedStoryCount { get; set; }
}
