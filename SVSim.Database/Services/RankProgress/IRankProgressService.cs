using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Services.RankProgress;

/// <summary>
/// Wire-shape result of a single rank-battle finish grant. The wire-mapped fields
/// (<see cref="Rank"/>, <see cref="AfterBattlePoint"/>, <see cref="AfterMasterPoint"/>,
/// <see cref="BattlePoint"/>, <see cref="MasterPoint"/>, <see cref="IsMasterRank"/>,
/// <see cref="IsGrandMasterRank"/>) map 1:1 to <c>RankBattleFinishResponseDto</c>
/// keys the client reads via GetValueOrDefault in Wizard/RankBattleFinishTask.cs:57-63.
/// <see cref="TierAdvanced"/> is a controller-side signal for the rank_achieved mission
/// emit and is not serialized to the wire.
/// </summary>
/// <param name="Rank">rank_id post-grant (1..29).</param>
/// <param name="AfterBattlePoint">Point post-grant (accumulated).</param>
/// <param name="AfterMasterPoint">MasterPoint post-grant (accumulated).</param>
/// <param name="BattlePoint">Signed Point delta (+100/-50/0).</param>
/// <param name="MasterPoint">Signed MasterPoint delta.</param>
/// <param name="IsMasterRank">True iff Rank == 25.</param>
/// <param name="IsGrandMasterRank">True iff Rank &gt;= 26.</param>
/// <param name="TierAdvanced">
/// True iff this grant crossed the viewer into a higher <see cref="RankTier"/> bucket
/// than they held pre-grant. Callsites gate rank_achieved mission emits on this flag.
/// </param>
public sealed record RankProgressResult(
    int Rank,
    int AfterBattlePoint,
    int AfterMasterPoint,
    int BattlePoint,
    int MasterPoint,
    bool IsMasterRank,
    bool IsGrandMasterRank,
    bool TierAdvanced = false);

public interface IRankProgressService
{
    /// <summary>
    /// Applies a +100 (win) or -50 (loss) delta to the viewer's rank progression in the
    /// given format, respecting tier floors from <c>ranks.csv</c>'s LowerLimitPoint column.
    /// Creates a ViewerRankProgress row for (viewer, format) if none exists. Caller must
    /// have loaded the viewer with <c>.Include(v =&gt; v.RankProgress)</c> and must call
    /// <c>SaveChangesAsync</c> after this returns.
    /// </summary>
    /// <param name="format">Must be Format.Rotation or Format.Unlimited.</param>
    Task<RankProgressResult> GrantAsync(
        Viewer viewer, Format format, bool isWin, CancellationToken ct = default);

    /// <summary>
    /// Read-only current progression snapshot for (viewer, format). Returns a zero-value
    /// result if no row exists. Does not mutate the viewer or hit the DB.
    /// </summary>
    Task<RankProgressResult> GetAsync(
        Viewer viewer, Format format, CancellationToken ct = default);
}
