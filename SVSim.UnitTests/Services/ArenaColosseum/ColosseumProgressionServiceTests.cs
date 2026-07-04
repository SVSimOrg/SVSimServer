using NUnit.Framework;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.EmulatedEntrypoint.Services.ArenaColosseum;

namespace SVSim.UnitTests.Services.ArenaColosseum;

/// <summary>
/// Pure-logic tests for the bracket-advancement / promotion / reward-bundle service.
/// No DB, no controllers, no HTTP — these are the only place to lock the spec README's
/// "&lt; FinalB" cap rule + the 3008 promotion trigger semantics.
/// </summary>
[TestFixture]
public class ColosseumProgressionServiceTests
{
    private static ColosseumRoundsConfig BuildThreeRoundConfig() => new()
    {
        Rounds = new()
        {
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 1,
                Groups = new() { new() { Group = "", MaxBattleCount = 5, BreakthroughNumber = 3, EntryNumber = 100_000 } },
                FinishRewards = new()
                {
                    new() { Type = UserGoodsType.Crystal, DetailId = 0, Count = 100, Name = "Round 1 bonus" },
                },
                RetireRewards = new()
                {
                    new() { Type = UserGoodsType.Rupy, DetailId = 0, Count = 50, Name = "Consolation" },
                },
            },
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 2,
                Groups = new() { new() { Group = "Group A", MaxBattleCount = 5, BreakthroughNumber = 4, EntryNumber = 10_000 } },
                FinishRewards = new()
                {
                    new() { Type = UserGoodsType.Crystal, DetailId = 0, Count = 250, Name = "Round 2 bonus" },
                },
            },
            new ColosseumRoundsConfig.RoundEntry
            {
                RoundId = 3,
                Groups = new() { new() { Group = "Final", MaxBattleCount = 5, BreakthroughNumber = 4, EntryNumber = 1_000 } },
                FinishRewards = new()
                {
                    new() { Type = UserGoodsType.Crystal, DetailId = 0, Count = 1000, Name = "Final clear" },
                },
            },
        },
        ChampionRewards = new()
        {
            new() { Type = UserGoodsType.Item, DetailId = 5, Count = 1, Name = "Champion Pack" },
        },
    };

    [Test]
    public void Win_threshold_advances_round_1_to_round_2()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun { RoundId = 1, WinCount = 3, BattleCountThisRound = 3 };

        var decision = svc.DecideAdvancement(run, rounds);

        Assert.That(decision.NextRoundId, Is.EqualTo(2));
        Assert.That(decision.IsBracketEnd, Is.False);
        Assert.That(decision.IsChampion, Is.False);
    }

    [Test]
    public void Loss_cap_ends_bracket_at_current_round()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun
        {
            RoundId = 2,
            WinCount = 3, // one short of breakthrough (4)
            BattleCountThisRound = 5, // hit the cap
            LossCount = 2,
        };

        var decision = svc.DecideAdvancement(run, rounds);

        Assert.That(decision.NextRoundId, Is.EqualTo(2));
        Assert.That(decision.IsBracketEnd, Is.True);
        Assert.That(decision.IsChampion, Is.False);
    }

    [Test]
    public void Final_round_breakthrough_marks_champion()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun
        {
            RoundId = 3,
            WinCount = 4, // hit breakthrough on the final round
            BattleCountThisRound = 4,
        };

        var decision = svc.DecideAdvancement(run, rounds);

        Assert.That(decision.NextRoundId, Is.EqualTo(3));
        Assert.That(decision.IsBracketEnd, Is.True);
        Assert.That(decision.IsChampion, Is.True);
    }

    [Test]
    public void ShouldPromoteToRankMatching_flips_once_on_3008()
    {
        var svc = new ColosseumProgressionService();
        var run = new ViewerArenaColosseumRun { IsRankMatching = false };

        Assert.That(svc.ShouldPromoteToRankMatching(run, 3004), Is.False,
            "3004 SUCCEEDED is not the promotion signal");
        Assert.That(svc.ShouldPromoteToRankMatching(run, 3008), Is.True,
            "3008 is the colosseum-specific promotion trigger per do-matching.md");

        run.IsRankMatching = true;
        Assert.That(svc.ShouldPromoteToRankMatching(run, 3008), Is.False,
            "already promoted — no second flip");
    }

    [Test]
    public void BuildRetireRewards_returns_round_specific_bundle()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun { RoundId = 1 };

        var rewards = svc.BuildRetireRewards(run, rounds);

        Assert.That(rewards.Count, Is.EqualTo(1));
        Assert.That(rewards[0].Type, Is.EqualTo(UserGoodsType.Rupy));
        Assert.That(rewards[0].Count, Is.EqualTo(50));
    }

    [Test]
    public void BuildRetireRewards_returns_empty_when_round_has_none()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        // Round 2 has no RetireRewards configured — server still emits an empty list per spec.
        var run = new ViewerArenaColosseumRun { RoundId = 2 };

        var rewards = svc.BuildRetireRewards(run, rounds);

        Assert.That(rewards, Is.Empty);
    }

    [Test]
    public void BuildFinishRewards_appends_champion_bundle_when_champion()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun { RoundId = 3, IsChampion = true };

        var rewards = svc.BuildFinishRewards(run, rounds);

        Assert.That(rewards.Count, Is.EqualTo(2), "round 3 finish + champion bundle");
        Assert.That(rewards.Any(r => r.Name == "Final clear"), Is.True);
        Assert.That(rewards.Any(r => r.Name == "Champion Pack"), Is.True);
    }

    [Test]
    public void BuildFinishRewards_omits_champion_bundle_when_not_champion()
    {
        var svc = new ColosseumProgressionService();
        var rounds = BuildThreeRoundConfig();
        var run = new ViewerArenaColosseumRun { RoundId = 1, IsChampion = false };

        var rewards = svc.BuildFinishRewards(run, rounds);

        Assert.That(rewards.Count, Is.EqualTo(1));
        Assert.That(rewards[0].Name, Is.EqualTo("Round 1 bonus"));
    }
}
