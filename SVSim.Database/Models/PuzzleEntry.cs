using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per basic_puzzle within a group. Static catalog seeded by SVSim.Bootstrap.
/// See docs/api-spec/endpoints/post-login/basic-puzzle/info.md (PuzzleEntry).
/// </summary>
public class PuzzleEntry : BaseEntity<int>
{
    /// <summary>puzzle_id on the wire. PK.</summary>
    public int PuzzleId { get => Id; set => Id = value; }

    /// <summary>FK to <see cref="PuzzleGroupEntry"/>. Index this column for mission evaluation.</summary>
    public int GroupId { get; set; }

    public PuzzleGroupEntry Group { get; set; } = null!;

    /// <summary>0..3 difficulty band.</summary>
    public int PuzzleDifficulty { get; set; }

    public bool IsAdditional { get; set; }
    public bool IsPlayable { get; set; } = true;
    public string ReleaseConditionTextId { get; set; } = string.Empty;
}
