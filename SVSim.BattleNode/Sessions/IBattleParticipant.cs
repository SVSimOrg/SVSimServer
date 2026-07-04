using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions;

/// <summary>
/// One side of a battle. Two of these are held by a <c>BattleSession</c>; the session
/// brokers between them. Concrete impls:
/// <list type="bullet">
///   <item><c>RealParticipant</c> — WS-backed (used for <c>BattleType.Pvp</c>).</item>
///   <item><c>NoOpBotParticipant</c> — silent; for <c>BattleType.Bot</c> (AI-passive).</item>
/// </list>
/// </summary>
public interface IBattleParticipant : IAsyncDisposable
{
    /// <summary>Real viewer id, or a synthetic stable id for bots
    /// (<see cref="Lifecycle.ServerBattleFrames.FakeOpponentViewerId"/>).</summary>
    long ViewerId { get; }

    /// <summary>Per-battle MatchContext snapshot, used for building Matched/BattleStart
    /// selfInfo when this participant is "self" in the perspective.</summary>
    MatchContext Context { get; }

    /// <summary>Session calls this to deliver a frame from the OTHER participant
    /// (or a server-synthesized broadcast). Real impl: encode + WS-send.
    /// NoOp: swallow.</summary>
    /// <param name="stock"><see cref="Stock.Bypass"/> for control frames (BattleFinish, JudgeResult,
    /// ack) — bypasses playSeq assignment + archive; <see cref="Stock.Normal"/> for gameplay frames.</param>
    Task PushAsync(MsgEnvelope envelope, Stock stock, CancellationToken ct);

    /// <summary>Participant fires this when it has a frame to send TO the session
    /// (its own gameplay action). Real impl: fires on WS recv. NoOp: never fires.</summary>
    event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;

    /// <summary>Drives the participant's inbound loop. For Real: the WS read loop
    /// (returns when the WS closes). For NoOp: completes immediately (the
    /// session keeps running as long as the OTHER participant's RunAsync is alive).</summary>
    Task RunAsync(CancellationToken ct);

    /// <summary>Called when the battle ends. Concrete impls clean up (close WS, etc.).</summary>
    Task TerminateAsync(BattleFinishReason reason);
}
