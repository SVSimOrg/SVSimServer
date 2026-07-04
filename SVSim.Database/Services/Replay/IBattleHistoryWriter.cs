namespace SVSim.Database.Services.Replay;

/// <summary>
/// Persists battle finishes to ViewerBattleHistory for the /replay/info list view.
/// </summary>
public interface IBattleHistoryWriter
{
    /// <summary>
    /// Insert a history row for (viewerId, ctx.BattleId). No-op when ctx is null
    /// (missing context = server restart mid-battle; warn-log and continue).
    /// Idempotent on the composite PK — duplicate calls skip silently.
    /// Enforces 50-row per-viewer retention by evicting the oldest CreateTime row
    /// when at cap before insert.
    /// </summary>
    Task RecordAsync(long viewerId, BattleContext? ctx, bool isWin, CancellationToken ct);
}
