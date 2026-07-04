using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Daily login bonus controller-facing API. Single entry point: called from
/// /load/index inside the existing inventory transaction. Returns null when the viewer
/// has already claimed today's bonus.
/// </summary>
public interface ILoginBonusService
{
    /// <summary>
    /// If due, advances <c>tx.Viewer.LoginBonusStreak</c> + <c>LastLoginBonusClaimedAt</c>,
    /// grants the day's reward via <paramref name="tx"/>, and returns the populated wire
    /// DTO. If not due, returns null and leaves viewer state untouched.
    /// </summary>
    Task<DailyLoginBonus?> GrantIfDueAsync(IInventoryTransaction tx, CancellationToken ct = default);

    /// <summary>Read-only "would GrantIfDueAsync emit a bonus right now?" — for MyPage flag.</summary>
    bool IsDue(SVSim.Database.Models.Viewer viewer);
}
