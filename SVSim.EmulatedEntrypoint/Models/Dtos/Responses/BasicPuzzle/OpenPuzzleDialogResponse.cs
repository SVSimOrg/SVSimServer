using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.BasicPuzzle;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BasicPuzzle;

[MessagePackObject]
public class OpenPuzzleDialogResponse
{
    [JsonPropertyName("puzzle_quest")] [Key("puzzle_quest")]
    public List<PuzzleEntryResponse> PuzzleQuest { get; set; } = new();

    [JsonPropertyName("puzzle_quest_chara_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("puzzle_quest_chara_id")]
    public int PuzzleQuestCharaId { get; set; }

    [JsonPropertyName("puzzle_difficulty_name_list")] [Key("puzzle_difficulty_name_list")]
    public Dictionary<string, string> PuzzleDifficultyNameList { get; set; } = new();

    [JsonPropertyName("is_display_badge")] [Key("is_display_badge")]
    public bool IsDisplayBadge { get; set; } = false;

    [JsonPropertyName("is_display_puzzle_new")] [Key("is_display_puzzle_new")]
    public bool IsDisplayPuzzleNew { get; set; } = false;
}
