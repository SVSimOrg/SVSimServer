using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Participants; // IHasHandshakePhase

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>Everything a handler reads or mutates for one inbound frame. <see cref="A"/>/<see cref="B"/>
/// are the session's positional participants (preserved so handlers that iterate participants in a
/// stable order — e.g. the mulligan barrier — match the legacy switch byte-for-byte). <see cref="From"/>
/// is the sender; <see cref="Other"/> is the non-sender.</summary>
internal sealed class FrameDispatchContext
{
    internal required IBattleParticipant A { get; init; }
    internal required IBattleParticipant B { get; init; }
    internal required IBattleParticipant From { get; init; }
    internal required IBattleParticipant Other { get; init; }
    internal required MsgEnvelope Env { get; init; }
    internal required string BattleId { get; init; }
    internal required BattleSessionState State { get; init; }

    /// <summary>The session's shadow engine (design ND2/F-N-6). In Phase-2 N1 it is fed in pure shadow
    /// and read by no handler; N2+ handlers source opponent-facing fields from it. Always non-null;
    /// <see cref="Engine.SessionBattleEngine.IsReady"/> is false until the engine is set up (and stays
    /// false if headless setup is unavailable in the host — the shadow then no-ops).</summary>
    internal required Engine.SessionBattleEngine Engine { get; init; }

    /// <summary>The opponent is an AI-passive (ack-only) bot: it runs no handshake — no
    /// <see cref="IHasHandshakePhase"/> — and receives no relayed frames (the client drives its own
    /// AI; the server only acks). This is the participant property that replaces the per-handler
    /// <c>BattleType.Bot</c> switch: the Bot dispatch arms gate on it. Its inverse — a live relay
    /// peer — is what <see cref="BothSidesAfterReady"/> already implies (only real peers have a
    /// handshake phase), so the relay arms need no separate opponent check.</summary>
    internal bool OpponentIsAckOnly => Other is not IHasHandshakePhase;

    /// <summary>The dispatching participant's handshake phase (null for a non-IHasHandshakePhase
    /// participant, e.g. NoOpBot). Setting it advances the sender.</summary>
    internal HandshakePhase? SenderPhase
    {
        get => (From as IHasHandshakePhase)?.Phase;
        set { if (From is IHasHandshakePhase p && value is { } v) p.Phase = v; }
    }

    /// <summary>Just the SENDER has finished the handshake — says nothing about the opponent. The
    /// Bot arms gate on this (the bot has no handshake phase of its own); contrast
    /// <see cref="BothSidesAfterReady"/>, which the PvP arms require. The sender-only vs both-sides
    /// distinction is load-bearing for the Bot/PvP split (see TurnEndHandler / TurnEndFinalHandler).</summary>
    internal bool SenderIsAfterReady => SenderPhase == HandshakePhase.AfterReady;

    /// <summary>BOTH participants have finished the handshake. Reads A/B (not From/Other) so the
    /// result is identical regardless of which side sent the frame. Contrast
    /// <see cref="SenderIsAfterReady"/> (sender only). Only a live relay peer (real player) has a
    /// handshake phase, so this can only be true in a two-real-player (PvP) session — the relay
    /// dispatch arms gate on this instead of a <c>BattleType</c> check (an ack-only bot opponent,
    /// <see cref="OpponentIsAckOnly"/>, can never satisfy it).</summary>
    internal bool BothSidesAfterReady() =>
        (A as IHasHandshakePhase)?.Phase == HandshakePhase.AfterReady &&
        (B as IHasHandshakePhase)?.Phase == HandshakePhase.AfterReady;
}
