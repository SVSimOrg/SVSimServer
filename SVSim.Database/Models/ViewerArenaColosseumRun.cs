using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One active Grand Prix (Arena Colosseum) bracket run per viewer. Mirrors
/// <see cref="ViewerArenaTwoPickRun"/> in shape so the 2-Pick draft state machine can be
/// lifted onto it in Phase 3 — the schema is the union of constructed-mode lifecycle
/// (registered decks, bracket counts, rank-match promotion flag) and TK2-style draft
/// state. Standalone (not a Viewer owned collection) per
/// project_ef_nav_include_pitfall, with a unique index on ViewerId to enforce
/// "one active run per viewer". Row is deleted on /finish or /retire.
/// </summary>
[Index(nameof(ViewerId), IsUnique = true)]
public class ViewerArenaColosseumRun
{
    public long Id { get; set; }

    public long ViewerId { get; set; }

    /// <summary>Wire <c>entry_info.id</c>. Set to <see cref="Id"/> on insert.</summary>
    public long EntryId { get; set; }

    /// <summary>Stamped from <see cref="Config.ColosseumSeasonConfig.SeasonId"/> at entry time so
    /// mid-run season-config edits don't shift the run's identity.</summary>
    public int SeasonId { get; set; }

    /// <summary>Current bracket round (1..3 in the canonical 3-round schedule). Indexes
    /// <see cref="Config.ColosseumRoundsConfig.Rounds"/> at entry time, advances on bracket
    /// promotion via <c>ColosseumProgressionService</c>.</summary>
    public int RoundId { get; set; }

    /// <summary>Format the bracket plays in (Rotation/Unlimited/TwoPick/HOF/WindFall/Avatar/...).
    /// Stamped from season config at entry time.</summary>
    public Format DeckFormat { get; set; }

    public long LeaderSkinId { get; set; }

    /// <summary>eARENA_PAY: 1 = ticket, 2 = crystal, 3 = rupy, 0 = free entry. Stamped at entry.</summary>
    public int ConsumeItemType { get; set; }

    // --- 2-Pick / Chaos draft state (lifted from ViewerArenaTwoPickRun for Phase 3) ---

    [Column(TypeName = "jsonb")]
    public string CandidateClassIdsJson { get; set; } = "[]";

    /// <summary>Stored as 0 in constructed mode (no draft turn machinery).</summary>
    public int SelectTurn { get; set; }

    public bool IsSelectCompleted { get; set; }

    [Column(TypeName = "jsonb")]
    public string SelectedCardIdsJson { get; set; } = "[]";

    [Column(TypeName = "jsonb")]
    public string PendingPickSetsJson { get; set; } = "[]";

    /// <summary>Monotonic counter for CandidatePair.Id; advances by 2 each draft turn.</summary>
    public long NextCandidateId { get; set; } = 1;

    /// <summary>Selected class for 2-Pick / Chaos modes; 0 in constructed mode.</summary>
    public int ClassId { get; set; }

    /// <summary>Optional Chaos sub-mode replay id. 0 when not in Chaos.</summary>
    public int ChaosId { get; set; }

    // --- Per-round bracket state ---

    [Column(TypeName = "jsonb")]
    public string ResultListJson { get; set; } = "[]";

    public int WinCount { get; set; }
    public int LossCount { get; set; }
    public int BattleCountThisRound { get; set; }

    /// <summary>Cap copied from the matching <c>ColosseumRoundsConfig.Rounds[RoundId-1].Groups[0]</c>
    /// at entry — stamped so mid-run round-config edits don't shift the cap.</summary>
    public int MaxBattleCountThisRound { get; set; }

    /// <summary>Wins required to break through to the next round. Same stamping rule as
    /// <see cref="MaxBattleCountThisRound"/>.</summary>
    public int BreakthroughNumberThisRound { get; set; }

    /// <summary>Remaining attempts in the current entry. Decremented per battle finish until
    /// 0 or breakthrough.</summary>
    public int RestEntryNum { get; set; }

    /// <summary>Flipped exactly once when the node signals <c>matching_state == 3008</c>.
    /// Subsequent battle URLs use the <c>colosseum_rank_battle/*</c> prefix.</summary>
    public bool IsRankMatching { get; set; }

    public bool IsChampion { get; set; }

    // --- Registered deck slot (constructed mode) ---

    [Column(TypeName = "jsonb")]
    public string RegisteredDeckNoListJson { get; set; } = "[]";

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
