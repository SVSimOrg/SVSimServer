namespace SVSim.BattleNode.Sessions;

/// <summary>
/// Discriminator for a pending battle and the session it produces. See
/// docs/superpowers/specs/2026-06-01-battle-node-v2-architecture-design.md.
/// </summary>
public enum BattleType
{
    /// <summary>Two real players. Server brokers between two WebSockets.
    /// Both <c>BattlePlayer</c> slots required.</summary>
    Pvp,

    /// <summary>One real player; opponent runs in the client (prod's IsAINetwork
    /// path; matched only in rank rotation / rank unlimited per prod). Server is
    /// ack-only. <c>p2</c> must be null.</summary>
    Bot,
}
