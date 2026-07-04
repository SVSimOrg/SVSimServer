namespace SVSim.BattleNode.Bridge;

/// <summary>
/// Known values for <see cref="MatchContext.BattleModeId"/> — the prod do_matching battle-mode id,
/// forwarded verbatim onto the wire (<c>battleType</c> field on BattleStart). Names the otherwise
/// magic <c>11</c>. Distinct from the <see cref="Sessions.BattleType"/> enum (Pvp/Bot), which is the
/// session topology, not the game mode.
/// </summary>
public static class BattleModes
{
    /// <summary>Take Two (TK2) — the two-pick draft mode the v1 captures were taken from. Prod
    /// rank-battle frames carry the same value (see <c>MatchContextBuilder</c>).</summary>
    public const int TakeTwo = 11;
}
