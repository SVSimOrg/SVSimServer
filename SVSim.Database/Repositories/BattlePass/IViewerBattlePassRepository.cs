using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BattlePass;

public interface IViewerBattlePassRepository
{
    /// <summary>
    /// Get-or-create progress row for (viewer, season). New rows are saved IMMEDIATELY (with
    /// DbUpdateException catch-and-retry to handle the concurrent-first-visit race against
    /// the (ViewerId, SeasonId) unique index). Existing rows are returned tracked so callers
    /// can mutate them and batch the save with other changes.
    /// </summary>
    Task<ViewerBattlePassProgressEntry> GetOrCreateProgressAsync(long viewerId, int seasonId, CancellationToken ct);

    /// <summary>
    /// All claim rows for (viewer, season). Used by /battle_pass/info to enrich is_received.
    /// </summary>
    Task<List<ViewerBattlePassClaimEntry>> GetClaimsAsync(long viewerId, int seasonId, CancellationToken ct);

    /// <summary>
    /// Append a claim row (in-memory; caller saves).
    /// </summary>
    void AddClaim(long viewerId, int seasonId, BattlePassTrack track, int level, DateTimeOffset claimedAt);
}
