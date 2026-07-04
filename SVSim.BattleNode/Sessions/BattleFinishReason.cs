namespace SVSim.BattleNode.Sessions;

/// <summary>Reason a participant was terminated. Carried to
/// <see cref="IBattleParticipant.TerminateAsync"/> so impls can log/clean differently
/// per cause. Cleanup itself is the same regardless of reason.</summary>
public enum BattleFinishReason
{
    NormalFinish,
    Retire,
    OpponentDisconnect,
    Timeout,
    ServerAbort,
}
