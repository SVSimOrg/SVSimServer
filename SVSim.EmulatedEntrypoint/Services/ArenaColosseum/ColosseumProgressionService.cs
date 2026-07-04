using SVSim.Database.Models;
using SVSim.Database.Models.Config;

namespace SVSim.EmulatedEntrypoint.Services.ArenaColosseum;

public class ColosseumProgressionService : IColosseumProgressionService
{
    /// <summary>Colosseum-specific node signal — see do-matching.md §matching_state 3008.</summary>
    public const int PromoteToRankMatchingState = 3008;

    public bool ShouldPromoteToRankMatching(ViewerArenaColosseumRun run, int matchingState) =>
        matchingState == PromoteToRankMatchingState && !run.IsRankMatching;

    public AdvancementDecision DecideAdvancement(ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds)
    {
        var currentRound = rounds.Rounds.FirstOrDefault(r => r.RoundId == run.RoundId);
        var currentGroup = currentRound?.Groups.FirstOrDefault();
        var maxRoundId = rounds.Rounds.Count == 0 ? run.RoundId : rounds.Rounds.Max(r => r.RoundId);

        // No matching round config (e.g. content not seeded) — treat current state as terminal
        // so the controller doesn't loop forever. Champion=false because we can't tell.
        if (currentGroup is null)
        {
            return new AdvancementDecision(run.RoundId, IsBracketEnd: true, IsChampion: false);
        }

        // Cleared the breakthrough threshold this round.
        if (run.WinCount >= currentGroup.BreakthroughNumber)
        {
            bool isFinal = run.RoundId >= maxRoundId;
            return new AdvancementDecision(
                NextRoundId: isFinal ? run.RoundId : run.RoundId + 1,
                IsBracketEnd: isFinal,
                IsChampion: isFinal);
        }

        // Exhausted the per-round battle cap without clearing — bracket ends at current round.
        if (run.BattleCountThisRound >= currentGroup.MaxBattleCount)
        {
            return new AdvancementDecision(run.RoundId, IsBracketEnd: true, IsChampion: false);
        }

        // Mid-round, still playing.
        return new AdvancementDecision(run.RoundId, IsBracketEnd: false, IsChampion: false);
    }

    public IReadOnlyList<ColosseumRoundsConfig.RewardEntry> BuildRetireRewards(
        ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds)
    {
        var round = rounds.Rounds.FirstOrDefault(r => r.RoundId == run.RoundId);
        return round?.RetireRewards ?? new();
    }

    public IReadOnlyList<ColosseumRoundsConfig.RewardEntry> BuildFinishRewards(
        ViewerArenaColosseumRun run, ColosseumRoundsConfig rounds)
    {
        var round = rounds.Rounds.FirstOrDefault(r => r.RoundId == run.RoundId);
        var bundle = new List<ColosseumRoundsConfig.RewardEntry>();
        if (round is not null)
        {
            bundle.AddRange(round.FinishRewards);
        }
        if (run.IsChampion)
        {
            bundle.AddRange(rounds.ChampionRewards);
        }
        return bundle;
    }
}
