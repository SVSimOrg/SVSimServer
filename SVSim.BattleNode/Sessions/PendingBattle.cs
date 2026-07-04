using SVSim.BattleNode.Bridge;

namespace SVSim.BattleNode.Sessions;

/// <summary>
/// Sparse pre-connect record. Carries the battle type + one or two players. The
/// WebSocket handler reads this to validate the incoming WS connect and to
/// construct the right participants.
/// </summary>
public sealed record PendingBattle(string BattleId, BattleType Type, BattlePlayer P1, BattlePlayer? P2);
