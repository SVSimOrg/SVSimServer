using SVSim.Database.Models;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IArenaTwoPickCardPoolService
{
    /// <summary>
    /// Returns exactly 2 candidate pairs for the requested turn. Ids assigned monotonically
    /// (startingPairId, startingPairId+1); set_num = 1, 2; isSelected = false.
    /// </summary>
    List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng);

    /// <summary>
    /// Pool-override variant — used by Arena Colosseum's 2-Pick mode, where the draft pool
    /// comes from the per-season <c>ColosseumSeasonConfig.PoolCardSetIds</c> rather than the
    /// global <c>ChallengeConfig.PoolCardSetIds</c>. Pass an empty/null list to fall back to
    /// the default-pool resolution (challenge → rotation).
    /// </summary>
    List<CandidatePair> GeneratePickSetsForTurn(
        int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds);
}
