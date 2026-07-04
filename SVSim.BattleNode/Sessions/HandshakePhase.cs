namespace SVSim.BattleNode.Sessions;

/// <summary>
/// Per-participant progression through the v1 server-authored setup handshake. Each side advances
/// InitNetwork → InitBattle → Loaded → Swap → AfterReady as the session acks its emits. Tracked
/// per participant via <see cref="Participants.IHasHandshakePhase"/>; the session reads the
/// SENDER's phase (<see cref="Dispatch.FrameDispatchContext.SenderPhase"/>) to gate which setup
/// frame to author next. Distinct from the session-global <see cref="SessionLifecycle"/> — this is
/// one axis per side, that is one axis per battle.
/// </summary>
public enum HandshakePhase
{
    AwaitingInitNetwork,
    AwaitingInitBattle,
    AwaitingLoaded,
    AwaitingSwap,
    AfterReady,
}
