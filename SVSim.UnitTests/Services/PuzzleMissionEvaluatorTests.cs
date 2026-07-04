using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class PuzzleMissionEvaluatorTests
{
    private static readonly PuzzleMissionEntry Round1 = new()
        { Id = 1, MissionName = "Clear all Round 1 puzzles", RequireNumber = 3, TargetPuzzleGroupId = 301 };
    private static readonly PuzzleMissionEntry SpecialAll = new()
        { Id = 2, MissionName = "Clear all Special Round puzzles", RequireNumber = 8, TargetPuzzleGroupId = null };

    private readonly PuzzleMissionEvaluator _e = new();

    [Test]
    public void Evaluate_unmapped_mission_always_zero()
    {
        var cleared = new Dictionary<int, HashSet<int>> { [316] = new() { 106, 107, 108 } };
        var result = _e.Evaluate(new[] { SpecialAll }, cleared);

        Assert.That(result.Single().TotalCount, Is.EqualTo(0));
        Assert.That(result.Single().IsAchieved, Is.False);
    }

    [Test]
    public void Evaluate_mapped_mission_counts_clears_in_target_group_capped()
    {
        var partial = new Dictionary<int, HashSet<int>> { [301] = new() { 37, 38 } };
        Assert.That(_e.Evaluate(new[] { Round1 }, partial).Single().TotalCount, Is.EqualTo(2));
        Assert.That(_e.Evaluate(new[] { Round1 }, partial).Single().IsAchieved, Is.False);

        var complete = new Dictionary<int, HashSet<int>> { [301] = new() { 37, 38, 39 } };
        Assert.That(_e.Evaluate(new[] { Round1 }, complete).Single().IsAchieved, Is.True);

        // Imagine a future where the group has more puzzles than RequireNumber — cap at RequireNumber.
        var over = new Dictionary<int, HashSet<int>> { [301] = new() { 37, 38, 39, 999 } };
        Assert.That(_e.Evaluate(new[] { Round1 }, over).Single().TotalCount, Is.EqualTo(3));
    }

    [Test]
    public void FreshlyCompleted_returns_only_missions_flipping_true()
    {
        var before = new Dictionary<int, HashSet<int>> { [301] = new() { 37, 38 } };
        var after  = new Dictionary<int, HashSet<int>> { [301] = new() { 37, 38, 39 } };

        var fresh = _e.FreshlyCompleted(new[] { Round1, SpecialAll }, before, after);
        Assert.That(fresh, Has.Count.EqualTo(1));
        Assert.That(fresh[0].Mission.Id, Is.EqualTo(Round1.Id));

        // Re-evaluating with same before==after returns no fresh completions.
        Assert.That(_e.FreshlyCompleted(new[] { Round1, SpecialAll }, after, after), Is.Empty);
    }
}
