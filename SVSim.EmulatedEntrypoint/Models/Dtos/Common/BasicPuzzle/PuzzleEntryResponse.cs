using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.BasicPuzzle;

[MessagePackObject]
public class PuzzleEntryResponse
{
    [JsonPropertyName("puzzle_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("puzzle_id")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("puzzle_difficulty")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("puzzle_difficulty")]
    public int PuzzleDifficulty { get; set; }

    [JsonPropertyName("is_cleared")] [Key("is_cleared")]
    public bool IsCleared { get; set; }

    [JsonPropertyName("is_additional")] [Key("is_additional")]
    public bool IsAdditional { get; set; }

    [JsonPropertyName("is_playable")] [Key("is_playable")]
    public bool IsPlayable { get; set; } = true;

    [JsonPropertyName("release_condition_text_id")] [Key("release_condition_text_id")]
    public string ReleaseConditionTextId { get; set; } = string.Empty;
}
