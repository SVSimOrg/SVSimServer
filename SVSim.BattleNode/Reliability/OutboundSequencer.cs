using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Reliability;

/// <summary>
/// Per-session outbound ledger. Assigns monotonic playSeq to ordered pushes and archives
/// them for future Resume retransmit (v2). No-stock control pushes (BattleFinish/JudgeResult/Resume)
/// are wrapped with no playSeq and skip the archive.
/// </summary>
public sealed class OutboundSequencer
{
    /// <summary>First playSeq assigned. Starts at 1, not 0 — 0 is reserved for no-stock /
    /// unsequenced pushes (which carry a null PlaySeq via <see cref="WrapNoStock"/>).</summary>
    private const long FirstPlaySeq = 1;

    private long _next = FirstPlaySeq;

    // Holds every ordered (stocked) push for the WHOLE match — there is no per-ack pruning, so it
    // grows with battle length × concurrent battles. Bounded only by Clear() in the terminate cascade.
    // Fine at current scale; if battles get long or concurrency scales, prune entries below the peer's
    // ack watermark here (contrast the inbound side, which is bounded by InboundTracker.WindowSize).
    private readonly Dictionary<long, MsgEnvelope> _archive = new();

    public IReadOnlyDictionary<long, MsgEnvelope> Archive => _archive;

    public MsgEnvelope AssignAndArchive(MsgEnvelope envelope)
    {
        var seq = _next++;
        var stamped = envelope with { PlaySeq = seq };
        _archive[seq] = stamped;
        return stamped;
    }

    public MsgEnvelope WrapNoStock(MsgEnvelope envelope) =>
        envelope with { PlaySeq = null };

    /// <summary>
    /// Drop all archived envelopes. Called from BattleSession's terminate cascade so
    /// the archive — the heavy state — is released the moment the battle ends, rather
    /// than waiting for the participant to be GC'd. <c>_next</c> is left untouched:
    /// a participant emitting after Clear is a bug, not a recovery case, but the seq
    /// stream stays monotonic so a stray emit doesn't silently re-use a playSeq value.
    /// </summary>
    public void Clear() => _archive.Clear();
}
