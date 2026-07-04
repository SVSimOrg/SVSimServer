using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PuzzleSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("group_id")] public int GroupId { get; set; }
    [JsonPropertyName("puzzle_difficulty")] public int PuzzleDifficulty { get; set; }
    [JsonPropertyName("is_additional")] public bool IsAdditional { get; set; }
    [JsonPropertyName("is_playable")] public bool IsPlayable { get; set; }
    [JsonPropertyName("release_condition_text_id")] public string ReleaseConditionTextId { get; set; } = "";
}
