using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PuzzleSeedingPipelineTests
{
    [Test]
    public async Task ImportsAllPuzzleGroupsAndPuzzles()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        var ctx = factory.Services.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.PuzzleGroups.CountAsync(), Is.EqualTo(25),
            "25 groups in the captured /basic_puzzle/info (puzzle_master_ids 1..9 plus 301..316)");
        Assert.That(await ctx.Puzzles.CountAsync(), Is.GreaterThan(100),
            "~110 puzzles total across all groups");

        // Spot-check group 301 (the Round-1 character group, contains puzzles 37/38/39).
        var g301 = await ctx.PuzzleGroups.Include(g => g.Puzzles).FirstAsync(g => g.Id == 301);
        Assert.That(g301.BasicTitleTextId, Is.EqualTo("Puzzle_QuestSelect_0301"));
        Assert.That(g301.PuzzleCharaId, Is.EqualTo(3704));
        Assert.That(g301.Puzzles.Select(p => p.Id).OrderBy(x => x), Is.EqualTo(new[] { 37, 38, 39 }));
        Assert.That(g301.DifficultyNameListJson, Does.Contain("\"Beginner\":\"0\""));
    }

    [Test]
    public async Task IsIdempotent()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        await factory.SeedGlobalsAsync(); // second run — must not duplicate

        var ctx = factory.Services.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.PuzzleGroups.CountAsync(), Is.EqualTo(25));
    }

    [Test]
    public async Task ImportsAllPuzzleMissionsWithRoundMapping()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        var ctx = factory.Services.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.PuzzleMissions.CountAsync(), Is.EqualTo(19),
            "19 entries in the captured /basic_puzzle/mission");

        // "Clear all Round 1 puzzles" -> target group 301 + AchievedMessage derived.
        var round1 = await ctx.PuzzleMissions.FirstAsync(m => m.MissionName == "Clear all Round 1 puzzles");
        Assert.That(round1.TargetPuzzleGroupId, Is.EqualTo(301));
        Assert.That(round1.AchievedMessage, Is.EqualTo("Cleared all Round 1 puzzles"));
        Assert.That(round1.RequireNumber, Is.EqualTo(3));
        Assert.That((int)round1.RewardType, Is.EqualTo(10));        // LeaderSkin
        Assert.That(round1.RewardDetailId, Is.EqualTo(3704L)); // chara_id matching group 301
        Assert.That(round1.RewardNumber, Is.EqualTo(1));

        // Special-Round mission -> TargetPuzzleGroupId is null (deferred per Phase 1).
        var special = await ctx.PuzzleMissions.FirstAsync(m => m.MissionName == "Clear all Special Round puzzles");
        Assert.That(special.TargetPuzzleGroupId, Is.Null);
    }
}
