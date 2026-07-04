using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.BasicPuzzle;

[MessagePackObject]
public class PuzzleGroupResponse
{
    [JsonPropertyName("puzzle_master_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("puzzle_master_id")]
    public int PuzzleMasterId { get; set; }

    [JsonPropertyName("puzzle_data")] [Key("puzzle_data")]
    public List<PuzzleEntryResponse> PuzzleData { get; set; } = new();

    [JsonPropertyName("puzzle_chara_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("puzzle_chara_id")]
    public int PuzzleCharaId { get; set; }

    [JsonPropertyName("puzzle_difficulty_name_list")] [Key("puzzle_difficulty_name_list")]
    public Dictionary<string, string> PuzzleDifficultyNameList { get; set; } = new();

    [JsonPropertyName("is_all_cleared")] [Key("is_all_cleared")]
    public bool IsAllCleared { get; set; }

    [JsonPropertyName("chara_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("chara_id")]
    public int CharaId { get; set; }

    [JsonPropertyName("sort_type")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("sort_type")]
    public int SortType { get; set; }

    [JsonPropertyName("basic_title_text_id")] [Key("basic_title_text_id")]
    public string BasicTitleTextId { get; set; } = string.Empty;

    [JsonPropertyName("is_mission_target")] [Key("is_mission_target")]
    public bool IsMissionTarget { get; set; }
}
