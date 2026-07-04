namespace SVSim.BattleNode.Sessions.Engine;

/// <summary>Outcome of feeding one client frame to the engine (design ND6). A divergence/reject is a
/// DETECTED-DESYNC EVENT surfaced to the caller — never silently absorbed. Phase-2 policy: log.</summary>
internal sealed record EngineIngestResult(bool Accepted, bool Diverged, string? RejectReason)
{
    public static EngineIngestResult Ok() => new(Accepted: true, Diverged: false, RejectReason: null);
    public static EngineIngestResult Reject(string reason) => new(Accepted: false, Diverged: true, RejectReason: reason);
}
