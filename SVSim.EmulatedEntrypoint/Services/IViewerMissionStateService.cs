namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Lazy-initializes viewer mission/achievement state. Idempotent. Called from
/// LoadController on every /load/index and as belt-and-braces from /mission/info.
/// Takes viewerId (not Viewer) so it works against both tracked and detached viewer loads.
/// Caller is responsible for SaveChangesAsync.
/// </summary>
public interface IViewerMissionStateService
{
    Task EnsureCurrentAsync(long viewerId, CancellationToken ct = default);
}
