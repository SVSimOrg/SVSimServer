using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class PuzzleClearRepositoryTests
{
    [Test]
    public async Task UpsertClear_inserts_then_updates_idempotently()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var repo = factory.Services.GetRequiredService<IPuzzleClearRepository>();

        var clearsBefore = await repo.GetClearedPuzzleIds(viewerId);
        Assert.That(clearsBefore, Is.Empty);

        await repo.UpsertClearAsync(viewerId, puzzleId: 37, retryCount: 2);
        await repo.UpsertClearAsync(viewerId, puzzleId: 37, retryCount: 0); // better clear; BestRetryCount should drop to 0
        await repo.UpsertClearAsync(viewerId, puzzleId: 38, retryCount: 1);

        var ids = await repo.GetClearedPuzzleIds(viewerId);
        Assert.That(ids, Is.EquivalentTo(new[] { 37, 38 }));

        var ctx = factory.Services.GetRequiredService<SVSimDbContext>();
        var row37 = await ctx.ViewerPuzzleClears.FirstAsync(c => c.ViewerId == viewerId && c.PuzzleId == 37);
        Assert.That(row37.BestRetryCount, Is.EqualTo(0), "BestRetryCount is min across all wins");
    }

    [Test]
    public async Task GetClearedPuzzleIdsByGroup_groups_by_FK()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync(); // need Puzzles table populated for GroupId FKs
        long viewerId = await factory.SeedViewerAsync();
        var repo = factory.Services.GetRequiredService<IPuzzleClearRepository>();

        await repo.UpsertClearAsync(viewerId, 37, 0);   // group 301
        await repo.UpsertClearAsync(viewerId, 38, 0);   // group 301
        await repo.UpsertClearAsync(viewerId, 64, 0);   // group 306

        var byGroup = await repo.GetClearedPuzzleIdsByGroup(viewerId);
        Assert.That(byGroup[301], Is.EquivalentTo(new[] { 37, 38 }));
        Assert.That(byGroup[306], Is.EquivalentTo(new[] { 64 }));
        Assert.That(byGroup.Keys, Does.Not.Contain(999));
    }
}
