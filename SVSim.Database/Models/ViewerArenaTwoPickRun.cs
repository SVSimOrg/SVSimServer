using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// One active Take Two run per viewer. Standalone (not a Viewer owned collection) to avoid
/// the EF nav-include pitfalls in project_ef_nav_include_pitfall and to keep /load/index cheap.
/// Row is deleted on /retire and /finish completion. Unique index on ViewerId enforces
/// "one active run per viewer".
/// <para>
/// Lists are stored as jsonb strings (<c>{Field}Json</c>) per the project's inline-JSON column
/// pattern (see DefaultDeckEntry.CardIdArray). Repos own (de)serialization.
/// </para>
/// </summary>
[Index(nameof(ViewerId), IsUnique = true)]
public class ViewerArenaTwoPickRun
{
    public long Id { get; set; }

    public long ViewerId { get; set; }

    /// <summary>Wire <c>entry_info.id</c> / <c>two_pick_entry_id</c>. Set to <see cref="Id"/> on insert.</summary>
    public long EntryId { get; set; }

    public int RewardScheduleId { get; set; }
    public int ChallengeId { get; set; }

    /// <summary>MAX(reward.WinCount) at creation time. Stamped on the row so mid-run reward-table edits don't change the cap.</summary>
    public int MaxBattleCount { get; set; }

    /// <summary>0 until /class_choose.</summary>
    public int ClassId { get; set; }

    /// <summary>0 until first battle; set to class default on /class_choose.</summary>
    public long LeaderSkinId { get; set; }

    [Column(TypeName = "jsonb")]
    public string CandidateClassIdsJson { get; set; } = "[]";

    /// <summary>1..15.</summary>
    public int SelectTurn { get; set; }

    public bool IsSelectCompleted { get; set; }

    [Column(TypeName = "jsonb")]
    public string SelectedCardIdsJson { get; set; } = "[]";

    [Column(TypeName = "jsonb")]
    public string PendingPickSetsJson { get; set; } = "[]";

    /// <summary>Monotonic counter for CandidatePair.Id; advances by 2 each draft turn.</summary>
    public long NextCandidateId { get; set; } = 1;

    [Column(TypeName = "jsonb")]
    public string ResultListJson { get; set; } = "[]";

    public int WinCount { get; set; }
    public int LossCount { get; set; }
    public bool IsRetire { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
