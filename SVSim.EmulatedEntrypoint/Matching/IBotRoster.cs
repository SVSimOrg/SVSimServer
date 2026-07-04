using SVSim.BattleNode.Bridge;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// Picks a bot opponent for an incoming AI rank battle. Used by
/// <c>RankBattleController.AiStart</c> to compose <c>oppo_info</c>.
/// </summary>
/// <remarks>
/// Backed by the <c>BotRoster</c> table (seeded from
/// <c>SVSim.Bootstrap/Data/seeds/bot-roster.json</c>). Edit the seed + re-run
/// <c>SVSim.Bootstrap</c> to change the pool without recompiling.
/// </remarks>
public interface IBotRoster
{
    /// <summary>
    /// Returns a bot profile. Deterministic per <paramref name="battleId"/> so a
    /// mid-flight retry of <c>/ai_&lt;fmt&gt;/start</c> picks the same opponent,
    /// but different battles get different bots.
    /// </summary>
    Task<AIBotProfile> PickAsync(MatchContext selfCtx, string battleId, CancellationToken ct = default);
}
