using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;

namespace SVSim.EmulatedEntrypoint.Services.ArenaColosseum;

/// <summary>
/// Pure-logic decisions for the Colosseum bracket lifecycle. Reads a
/// <see cref="ViewerArenaColosseumRun"/> + <see cref="ColosseumRoundsConfig"/> snapshot and
/// returns advancement / promotion / reward decisions. Side-effectful (debits, grants,
/// run-row writes) live on the controllers — this service just computes.
/// </summary>
public interface IColosseumProgressionService
{
    /// <summary>True when the node signal <c>matching_state == 3008</c> indicates the run
    /// has been promoted to the ranked bracket and we haven't already flipped the flag.
    /// Subsequent battle URLs route to <c>colosseum_rank_battle/*</c>.</summary>
    bool ShouldPromoteToRankMatching(ViewerArenaColosseumRun run, int matchingState);

    /// <summary>Triggered post-match-finish when wins/losses cross thresholds. Returns the
    /// next round id (or current if no change), whether the bracket has ended, and the
    /// champion flag for the final-round-cleared case.</summary>
    AdvancementDecision DecideAdvancement(ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds);

    /// <summary>Bundle to grant on <c>/retire</c>. Reads
    /// <see cref="ColosseumRoundsConfig.RoundEntry.RetireRewards"/> for the run's
    /// <see cref="ViewerArenaColosseumRun.RoundId"/>. Empty when no matching round.</summary>
    IReadOnlyList<ColosseumRoundsConfig.RewardEntry> BuildRetireRewards(
        ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds);

    /// <summary>Bundle to grant on <c>/finish</c>. Combines the current round's
    /// <see cref="ColosseumRoundsConfig.RoundEntry.FinishRewards"/> with
    /// <see cref="ColosseumRoundsConfig.ChampionRewards"/> when the run is a champion.</summary>
    IReadOnlyList<ColosseumRoundsConfig.RewardEntry> BuildFinishRewards(
        ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds);
}

/// <summary>Output of <see cref="IColosseumProgressionService.DecideAdvancement"/>.</summary>
public sealed record AdvancementDecision(int NextRoundId, bool IsBracketEnd, bool IsChampion);
