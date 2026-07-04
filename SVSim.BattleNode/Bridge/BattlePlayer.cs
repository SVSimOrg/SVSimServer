namespace SVSim.BattleNode.Bridge;

/// <summary>One player slot for a pending battle. Carries the viewer's identity and
/// the per-battle MatchContext snapshot built at do_matching time.
/// <para>FOOTGUN: this is a <c>record</c>, but <see cref="Context"/> transitively holds an
/// <c>IReadOnlyList&lt;long&gt;</c> (the deck), so the synthesized value-equality is REFERENCE-based
/// on that list — two BattlePlayers with equal deck *contents* compare unequal. Don't use
/// BattlePlayer / <see cref="MatchContext"/> as dictionary keys or <c>Distinct()</c> / <c>HashSet</c>
/// operands without first giving them content equality. Not exercised today.</para></summary>
public sealed record BattlePlayer(long ViewerId, MatchContext Context);
