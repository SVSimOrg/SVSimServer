namespace SVSim.BattleNode.Sessions;

/// <summary>
/// How a pushed frame interacts with the per-participant <c>OutboundSequencer</c>: whether it
/// gets a <c>playSeq</c> and is archived for ordered replay, or bypasses both. Replaces a bare
/// (and negatively-named) <c>bool noStock</c> threaded through <see cref="IBattleParticipant.PushAsync"/>
/// and <see cref="Dispatch.DispatchRoute"/> — the literal <c>true</c>/<c>false</c> at call sites gave
/// no hint which sense was which, and was trivial to invert.
/// </summary>
public enum Stock
{
    /// <summary>Gameplay frame: assign the next <c>playSeq</c> and archive it for ordered replay.</summary>
    Normal = 0,

    /// <summary>Control frame (BattleFinish, JudgeResult, ack): bypass <c>playSeq</c> assignment + archive.</summary>
    Bypass = 1,
}
