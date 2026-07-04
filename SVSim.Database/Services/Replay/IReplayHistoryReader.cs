namespace SVSim.Database.Services.Replay;

public interface IReplayHistoryReader
{
    /// <summary>Newest-first by CreateTime. Caps at <paramref name="take"/> (default 50).</summary>
    Task<IReadOnlyList<ReplayHistoryEntry>> GetRecentAsync(long viewerId, int take, CancellationToken ct);
}
