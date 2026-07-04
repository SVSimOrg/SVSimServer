using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PuzzleGroupSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("basic_title_text_id")] public string BasicTitleTextId { get; set; } = "";
    [JsonPropertyName("puzzle_chara_id")] public int PuzzleCharaId { get; set; }
    [JsonPropertyName("chara_id")] public int CharaId { get; set; }
    [JsonPropertyName("sort_type")] public int SortType { get; set; }
    [JsonPropertyName("difficulty_name_list")] public JsonElement DifficultyNameList { get; set; }
}
