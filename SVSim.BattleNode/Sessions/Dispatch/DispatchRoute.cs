using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>One routing decision: deliver <paramref name="Frame"/> to <paramref name="Target"/>.
/// Named form of the tuple <c>ComputeFrames</c> historically returned. <paramref name="Stock"/>
/// is <see cref="Sessions.Stock.Bypass"/> for control frames (BattleFinish, ack) — bypasses
/// playSeq assignment + archive — and <see cref="Sessions.Stock.Normal"/> for gameplay frames.</summary>
internal readonly record struct DispatchRoute(IBattleParticipant Target, MsgEnvelope Frame, Stock Stock);
