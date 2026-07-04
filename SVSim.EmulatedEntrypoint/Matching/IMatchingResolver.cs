using SVSim.BattleNode.Bridge;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// Single source of truth for how a <c>/do_matching</c> request is resolved into a wire
/// matching_state + battle_id + node_server_url across every battle family.
/// <para>
/// Lives here (and not on each controller) because the resolution rules are the same
/// regardless of which URL family carried the request:
/// </para>
/// <list type="number">
///   <item>Consult <see cref="IMatchingPairUpService"/> and translate the
///         resulting <see cref="PairUpResult"/> into a wire matching_state per the
///         3002 / 3004 / 3007 / 3011 vocabulary.</item>
/// </list>
/// <para>
/// Family-specific details (DTO shapes, family-specific request fields like
/// <c>card_master_id</c>, error-mapping like rank-battle's 3001 on a missing deck) stay
/// on the controllers. The resolver only owns the cross-cutting "did the flag win, did
/// pair-up resolve, what's the state code" decision.
/// </para>
/// </summary>
public interface IMatchingResolver
{
    /// <param name="mode">
    /// The matching-mode key the resolver passes through to
    /// <see cref="IMatchingPairUpService.TryPairAsync"/> — one of the
    /// <see cref="ModePolicy"/> registry's mode names (e.g. <c>"arena_two_pick_battle"</c>,
    /// <c>"rotation_rank_battle"</c>, <c>"unlimited_rank_battle"</c>).
    /// </param>
    /// <param name="player">Caller's <see cref="BattlePlayer"/> (viewer-id + built MatchContext).</param>
    Task<MatchingResolution> ResolveAsync(
        string mode,
        BattlePlayer player,
        CancellationToken ct);
}

/// <summary>
/// Wire-level outcome of a <c>/do_matching</c> resolution. Always carries a non-null
/// <see cref="NodeServerUrl"/> — empty string while parked (3002), real URL on resolution —
/// because the client's <c>DoMatchingBase.SettingDoMatchingData()</c> calls
/// <c>.ToString()</c> on the wire field without a <c>Keys.Contains</c> guard.
/// </summary>
public sealed record MatchingResolution(int MatchingState, string? BattleId, string NodeServerUrl);
