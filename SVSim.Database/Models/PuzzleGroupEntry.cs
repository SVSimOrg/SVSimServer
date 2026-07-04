using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per basic_puzzle group (puzzle_master_id). Static catalog seeded by
/// SVSim.Bootstrap.PuzzleImporter from seeds/puzzle-groups.json.
/// See docs/api-spec/endpoints/post-login/basic-puzzle/info.md.
/// </summary>
public class PuzzleGroupEntry : BaseEntity<int>
{
    /// <summary>puzzle_master_id on the wire. PK + display order key.</summary>
    public int PuzzleMasterId { get => Id; set => Id = value; }

    /// <summary>SystemText id. "Puzzle_QuestSelect_0301" etc. Client resolves with Data.SystemText.Get.</summary>
    public string BasicTitleTextId { get; set; } = string.Empty;

    /// <summary>Character id for the group portrait. Wire as string but stored as int.</summary>
    public int PuzzleCharaId { get; set; }

    /// <summary>Mission-attribution chara. Usually == PuzzleCharaId but observed group 2 has 3208/2703 split.</summary>
    public int CharaId { get; set; }

    /// <summary>1 = Special/Expert rounds, 2 = Regular numbered rounds. Drives client display ordering.</summary>
    public int SortType { get; set; }

    /// <summary>Difficulty-name dict serialized as JSON (e.g. {"Beginner":"0","Experienced":"1","Expert":"2"}).</summary>
    public string DifficultyNameListJson { get; set; } = "{}";

    // Navigation
    public List<PuzzleEntry> Puzzles { get; set; } = new();
}
