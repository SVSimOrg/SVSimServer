using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PuzzleImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task ImportsGroups_PuzzlesAndMissions_from_seed_files()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new PuzzleImporter();
        await importer.ImportGroupsAsync(db, SeedDir);
        await importer.ImportPuzzlesAsync(db, SeedDir);
        await importer.ImportMissionsAsync(db, SeedDir);

        int groupCount = await db.PuzzleGroups.CountAsync();
        int puzzleCount = await db.Puzzles.CountAsync();
        int missionCount = await db.PuzzleMissions.CountAsync();

        Assert.That(groupCount, Is.GreaterThan(0), "seed must contain groups");
        Assert.That(puzzleCount, Is.GreaterThan(0), "seed must contain puzzles");
        Assert.That(missionCount, Is.GreaterThan(0), "seed must contain missions");

        // Every puzzle's GroupId must reference an existing group (FK satisfied).
        var groupIds = await db.PuzzleGroups.Select(g => g.Id).ToListAsync();
        var groupIdSet = new HashSet<int>(groupIds);
        var puzzleGroupIds = await db.Puzzles.Select(p => p.GroupId).Distinct().ToListAsync();
        foreach (var gid in puzzleGroupIds)
        {
            Assert.That(groupIdSet, Does.Contain(gid),
                $"puzzle references unknown group_id={gid}");
        }
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new PuzzleImporter();
        await importer.ImportGroupsAsync(db, SeedDir);
        await importer.ImportPuzzlesAsync(db, SeedDir);
        await importer.ImportMissionsAsync(db, SeedDir);

        int g1 = await db.PuzzleGroups.CountAsync();
        int p1 = await db.Puzzles.CountAsync();
        int m1 = await db.PuzzleMissions.CountAsync();

        await importer.ImportGroupsAsync(db, SeedDir);
        await importer.ImportPuzzlesAsync(db, SeedDir);
        await importer.ImportMissionsAsync(db, SeedDir);

        Assert.That(await db.PuzzleGroups.CountAsync(), Is.EqualTo(g1));
        Assert.That(await db.Puzzles.CountAsync(), Is.EqualTo(p1));
        Assert.That(await db.PuzzleMissions.CountAsync(), Is.EqualTo(m1));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyGroupId = 99999;
        const int legacyPuzzleId = 99998;
        const int legacyMissionId = 99997;

        db.PuzzleGroups.Add(new PuzzleGroupEntry
        {
            Id = legacyGroupId,
            BasicTitleTextId = "legacy_group",
            DifficultyNameListJson = "{\"legacy\":\"1\"}",
        });
        db.Puzzles.Add(new PuzzleEntry
        {
            Id = legacyPuzzleId,
            GroupId = legacyGroupId,
            PuzzleDifficulty = 5,
            ReleaseConditionTextId = "legacy_puzzle",
        });
        db.PuzzleMissions.Add(new PuzzleMissionEntry
        {
            Id = legacyMissionId,
            MissionName = "legacy_mission",
            AchievedMessage = "legacy_achieved",
            RequireNumber = 42,
        });
        await db.SaveChangesAsync();

        var importer = new PuzzleImporter();
        await importer.ImportGroupsAsync(db, SeedDir);
        await importer.ImportPuzzlesAsync(db, SeedDir);
        await importer.ImportMissionsAsync(db, SeedDir);

        var g = await db.PuzzleGroups.FindAsync(legacyGroupId);
        Assert.That(g, Is.Not.Null);
        Assert.That(g!.BasicTitleTextId, Is.EqualTo("legacy_group"));

        var p = await db.Puzzles.FindAsync(legacyPuzzleId);
        Assert.That(p, Is.Not.Null);
        Assert.That(p!.PuzzleDifficulty, Is.EqualTo(5));

        var m = await db.PuzzleMissions.FindAsync(legacyMissionId);
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.MissionName, Is.EqualTo("legacy_mission"));
        Assert.That(m.RequireNumber, Is.EqualTo(42));
    }

    [Test]
    public async Task Skips_rows_with_zero_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "puzzle-groups.json"),
                "[{\"id\":0,\"basic_title_text_id\":\"junk\"}]");
            File.WriteAllText(Path.Combine(tmp, "puzzles.json"),
                "[{\"id\":0,\"group_id\":1,\"puzzle_difficulty\":1}]");
            File.WriteAllText(Path.Combine(tmp, "puzzle-missions.json"),
                "[{\"id\":0,\"mission_name\":\"junk\"}]");

            var importer = new PuzzleImporter();
            await importer.ImportGroupsAsync(db, tmp);
            await importer.ImportPuzzlesAsync(db, tmp);
            await importer.ImportMissionsAsync(db, tmp);

            Assert.That(await db.PuzzleGroups.CountAsync(), Is.EqualTo(0),
                "rows with id=0 must not be inserted into groups");
            Assert.That(await db.Puzzles.CountAsync(), Is.EqualTo(0),
                "rows with id=0 must not be inserted into puzzles");
            Assert.That(await db.PuzzleMissions.CountAsync(), Is.EqualTo(0),
                "rows with id=0 must not be inserted into missions");
        }
        finally { Directory.Delete(tmp, true); }
    }
}
