namespace SVSim.BattleNode.Sessions;

/// <summary>
/// Session-global lifecycle. A battle stays <see cref="Active"/> until a terminal event — a lethal
/// TurnEndFinal, a Retire/Kill, or the disconnect drop cascade — flips it to <see cref="Terminal"/>,
/// after which the drop cascade will not synthesize another BattleFinish. Distinct from the
/// per-participant <see cref="HandshakePhase"/> (which side reached which setup step); this is one
/// axis per battle. Only these two states are load-bearing — the handshake progression lives on the
/// other enum.
/// </summary>
public enum SessionLifecycle
{
    Active,
    Terminal,
}
