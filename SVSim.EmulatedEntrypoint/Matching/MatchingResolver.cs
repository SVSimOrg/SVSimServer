using SVSim.BattleNode.Bridge;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <inheritdoc cref="IMatchingResolver"/>
public sealed class MatchingResolver : IMatchingResolver
{
    private readonly IMatchingBridge _bridge;
    private readonly IMatchingPairUpService _pairUp;

    public MatchingResolver(
        IMatchingBridge bridge,
        IMatchingPairUpService pairUp)
    {
        _bridge = bridge;
        _pairUp = pairUp;
    }

    public Task<MatchingResolution> ResolveAsync(
        string mode,
        BattlePlayer player,
        CancellationToken ct)
    {
        return ResolveViaPairUpAsync(mode, player, ct);
    }

    private async Task<MatchingResolution> ResolveViaPairUpAsync(string mode, BattlePlayer player, CancellationToken ct)
    {
        var paired = await _pairUp.TryPairAsync(mode, player, ct);
        if (paired is null)
        {
            // Parked. matching_state 3002 RETRY. node_server_url MUST be present as empty
            // string (the client unguarded-.ToString()s it before consulting matching_state).
            return new MatchingResolution(3002, BattleId: null, "");
        }

        // 3011 = AI_BATTLE_MATCHING_SUCCEEDED  (PvpFirstThenAiFallback policy's threshold fired)
        // 3007 = RC_BATTLE_MATCHING_SUCCEEDED_OWNER  (first arriver, cache pickup)
        // 3004 = RC_BATTLE_MATCHING_SUCCEEDED        (joiner — triggered the pair)
        var state = paired switch
        {
            { IsAiFallback: true } => 3011,
            { IsOwner: true } => 3007,
            _ => 3004,
        };
        return new MatchingResolution(state, paired.Match.BattleId, paired.Match.NodeServerUrl);
    }
}
