namespace SVSim.Database.Services.Replay;

/// <summary>
/// In-memory per-viewer battle context store. Bridges the start-time → finish-time
/// gap: the /finish request body carries neither battle_id nor opponent identity,
/// so this stash holds everything the finish hook needs to compose a
/// ViewerBattleHistory row.
/// </summary>
public interface IBattleContextStore
{
    /// <summary>Store the viewer's active battle context. Overwrites any prior entry.</summary>
    void Set(long viewerId, BattleContext ctx);

    /// <summary>Atomic read+clear. Returns null when no context (server restart,
    /// non-tracked family, already taken). Finish handlers must tolerate null.</summary>
    BattleContext? TakeFor(long viewerId);
}
