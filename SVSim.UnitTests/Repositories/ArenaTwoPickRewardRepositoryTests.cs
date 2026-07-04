using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Repositories.Globals;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class ArenaTwoPickRewardRepositoryTests
{
    private static async Task<SVSimDbContext> SeededContextAsync()
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();
        await new ArenaTwoPickRewardImporter()
            .ImportAsync(db, Path.Combine(AppContext.BaseDirectory, "Data", "seeds"));
        return db;
    }

    [Test]
    public async Task GetRewardsByWinCount_returns_two_rows_for_each_win_count()
    {
        await using var db = await SeededContextAsync();
        var repo = new ArenaTwoPickRewardRepository(db);

        for (int w = 0; w <= 5; w++)
        {
            var rows = await repo.GetRewardsByWinCountAsync(w);
            Assert.That(rows.Count, Is.EqualTo(2), $"WinCount={w} should have 2 reward rows");
        }
    }

    [Test]
    public async Task GetMaxWinCount_returns_5()
    {
        await using var db = await SeededContextAsync();
        var repo = new ArenaTwoPickRewardRepository(db);
        var max = await repo.GetMaxWinCountAsync();
        Assert.That(max, Is.EqualTo(5));
    }

    [Test]
    public async Task GetMaxWinCount_returns_0_when_catalog_empty()
    {
        var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();
        var repo = new ArenaTwoPickRewardRepository(db);
        var max = await repo.GetMaxWinCountAsync();
        Assert.That(max, Is.EqualTo(0));
    }
}
