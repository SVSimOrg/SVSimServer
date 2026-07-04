using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.BattleNode.Hosting;

/// <summary>
/// Per-BattleId WS rendezvous for PvP. First arriver parks; second arriver pairs.
/// The handler reads the result and either constructs the session (second arriver)
/// or awaits termination via the participant's session-finished signal (first arriver).
/// </summary>
public interface IWaitingRoom
{
    /// <summary>Try to claim a previously-parked first arriver. Returns the first
    /// arriver (and clears the slot) if one is parked; null if this caller is the
    /// first arriver (caller should then ParkAsync).</summary>
    RealParticipant? Pair(string battleId, RealParticipant self);

    /// <summary>Park as the first arriver; await pairing or timeout. Returns the
    /// second arriver on pairing; null on timeout / cancellation / TryAdd race.</summary>
    Task<RealParticipant?> ParkAsync(string battleId, RealParticipant self,
        TimeSpan timeout, CancellationToken ct);

    /// <summary>Best-effort cleanup; idempotent. Called on timeout or cancellation
    /// so a stale TCS doesn't linger if the first arriver disconnects before
    /// pairing.</summary>
    void Evict(string battleId);
}
