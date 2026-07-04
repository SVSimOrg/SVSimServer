using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Repositories.Globals;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class PuzzleCatalogRepositoryTests
{
    [Test]
    public async Task GetAllGroupsWithPuzzles_returns_25_groups_each_with_puzzles()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        var repo = factory.Services.GetRequiredService<IPuzzleCatalogRepository>();

        var groups = await repo.GetAllGroupsWithPuzzles();

        Assert.That(groups, Has.Count.EqualTo(25));
        Assert.That(groups.All(g => g.Puzzles.Count > 0), Is.True,
            "every group must have its Puzzles navigation populated");
        var g301 = groups.Single(g => g.Id == 301);
        Assert.That(g301.Puzzles.Select(p => p.Id).OrderBy(x => x), Is.EqualTo(new[] { 37, 38, 39 }));
    }

    [Test]
    public async Task GetGroupWithPuzzles_returns_one_group_or_null()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        var repo = factory.Services.GetRequiredService<IPuzzleCatalogRepository>();

        var g = await repo.GetGroupWithPuzzles(301);
        Assert.That(g, Is.Not.Null);
        Assert.That(g!.Puzzles, Has.Count.EqualTo(3));

        var missing = await repo.GetGroupWithPuzzles(99999);
        Assert.That(missing, Is.Null);
    }

    [Test]
    public async Task GetAllMissionsOrdered_returns_19_missions_in_correct_order()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        var repo = factory.Services.GetRequiredService<IPuzzleCatalogRepository>();

        var missions = await repo.GetAllMissionsOrdered();
        Assert.That(missions, Has.Count.EqualTo(19));

        // Captured order: by OrderId asc, then CampaignCommenceTime desc.
        var pairs = missions.Select(m => (m.OrderId, m.CampaignCommenceTime)).ToList();
        for (int i = 1; i < pairs.Count; i++)
        {
            var prev = pairs[i - 1]; var cur = pairs[i];
            Assert.That(prev.OrderId, Is.LessThanOrEqualTo(cur.OrderId));
            if (prev.OrderId == cur.OrderId)
                Assert.That(prev.CampaignCommenceTime, Is.GreaterThanOrEqualTo(cur.CampaignCommenceTime));
        }
    }
}
