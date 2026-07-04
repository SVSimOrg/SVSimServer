using SVSim.BattleNode.Sessions;

namespace SVSim.BattleNode.Bridge;

public interface IMatchingBridge
{
    /// <summary>
    /// Mint a battle id, register a pending session, return the URL the client should
    /// open a socket to.
    /// </summary>
    /// <remarks>
    /// Contract rules (enforced; throws <see cref="ArgumentException"/>):
    /// <list type="bullet">
    ///   <item><c>Pvp</c>: <paramref name="p2"/> required. Both viewers expected to
    ///         connect WS within 60s.</item>
    ///   <item><c>Bot</c>: <paramref name="p2"/> must be null. One viewer expected;
    ///         opponent runs in client.</item>
    /// </list>
    /// </remarks>
    PendingMatch RegisterBattle(BattlePlayer p1, BattlePlayer? p2, BattleType type);
}

public sealed record PendingMatch(string BattleId, string NodeServerUrl);
