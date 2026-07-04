using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer record of a cleared puzzle. Composite PK (ViewerId, PuzzleId) — at most one
/// row per (viewer, puzzle). NOT a Viewer owned collection on purpose (see CLAUDE.md
/// "EF nav include pitfall" — owned collection joins cartesian-explode the viewer graph).
/// </summary>
[PrimaryKey(nameof(ViewerId), nameof(PuzzleId))]
public class ViewerPuzzleClear
{
    public long ViewerId { get; set; }
    public int PuzzleId { get; set; }

    public DateTime ClearedAt { get; set; }

    /// <summary>Min retry_count across all wins. RetryCount = in-battle reset count, not loss retries.</summary>
    public int BestRetryCount { get; set; }
}
