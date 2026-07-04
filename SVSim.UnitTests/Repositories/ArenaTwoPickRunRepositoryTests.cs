using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class ArenaTwoPickRunRepositoryTests
{
    private static async Task<(SVSimDbContext, ArenaTwoPickRunRepository)> SetupAsync()
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();
        return (db, new ArenaTwoPickRunRepository(db));
    }

    [Test]
    public async Task GetByViewerId_returns_null_when_no_run()
    {
        var (db, repo) = await SetupAsync();
        await using var _ = db;
        var run = await repo.GetByViewerIdAsync(viewerId: 42);
        Assert.That(run, Is.Null);
    }

    [Test]
    public async Task Insert_then_fetch_round_trips_jsonb_columns()
    {
        var (db, repo) = await SetupAsync();
        await using var _ = db;

        var run = new ViewerArenaTwoPickRun
        {
            ViewerId = 42,
            EntryId = 0,
            RewardScheduleId = 1,
            ChallengeId = 1,
            MaxBattleCount = 7,
            CandidateClassIdsJson = "[1,7,8]",
            SelectedCardIdsJson = "[]",
            PendingPickSetsJson = "[]",
            ResultListJson = "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await repo.UpsertAsync(run);

        var fetched = await repo.GetByViewerIdAsync(42);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.CandidateClassIdsJson, Is.EqualTo("[1,7,8]"));
        Assert.That(fetched.MaxBattleCount, Is.EqualTo(7));
    }

    [Test]
    public async Task Delete_removes_the_row()
    {
        var (db, repo) = await SetupAsync();
        await using var _ = db;
        await repo.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 42, EntryId = 0, MaxBattleCount = 7,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await repo.DeleteAsync(42);
        var fetched = await repo.GetByViewerIdAsync(42);
        Assert.That(fetched, Is.Null);
    }
}
