using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Pack;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class PackRepositoryTests
{
    private static async Task SeedPack(SVSimTestFactory factory, int parentId, int baseId, PackCategory cat,
        DateTime commence, DateTime complete)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.Packs.Add(new PackConfigEntry
        {
            Id = parentId, BasePackId = baseId, PackCategory = cat,
            CommenceDate = commence, CompleteDate = complete,
            GachaType = 1, GachaDetail = "test",
            ChildGachas = { new PackChildGachaEntry { GachaId = parentId * 10, TypeDetail = CardPackType.RupyMulti, Cost = 100, CardCount = 8 } },
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task GetActivePacks_filters_by_date_window()
    {
        using var factory = new SVSimTestFactory();
        var now = new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc);
        await SeedPack(factory, 10001, 10001, PackCategory.None, now.AddDays(-30), now.AddDays(30));   // active
        await SeedPack(factory, 10002, 10002, PackCategory.None, now.AddDays(+1),  now.AddDays(30));   // not started
        await SeedPack(factory, 10003, 10003, PackCategory.None, now.AddDays(-30), now.AddDays(-1));   // expired

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPackRepository>();
        var packs = await repo.GetActivePacks(now);

        Assert.That(packs.Select(p => p.Id), Is.EquivalentTo(new[] { 10001 }));
    }

    [Test]
    public async Task GetActivePacks_excludes_IsEnabled_false_rows()
    {
        using var factory = new SVSimTestFactory();
        var now = new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc);
        await SeedPack(factory, 10001, 10001, PackCategory.None, now.AddDays(-1), now.AddDays(1));

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10002, BasePackId = 10002, PackCategory = PackCategory.None,
                CommenceDate = now.AddDays(-1), CompleteDate = now.AddDays(1),
                GachaType = 1, GachaDetail = "disabled",
                IsEnabled = false,
            });
            await db.SaveChangesAsync();
        }

        using var scopeRepo = factory.Services.CreateScope();
        var repo = scopeRepo.ServiceProvider.GetRequiredService<IPackRepository>();
        var packs = await repo.GetActivePacks(now);

        Assert.That(packs.Select(p => p.Id), Is.EquivalentTo(new[] { 10001 }));
    }

    [Test]
    public async Task GetPack_includes_child_gachas_and_banners()
    {
        using var factory = new SVSimTestFactory();
        var now = DateTime.UtcNow;
        await SeedPack(factory, 10001, 10001, PackCategory.None, now.AddDays(-1), now.AddDays(1));

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPackRepository>();
        var pack = await repo.GetPack(10001);

        Assert.That(pack, Is.Not.Null);
        Assert.That(pack!.ChildGachas.Count, Is.EqualTo(1));
        Assert.That(pack.ChildGachas[0].GachaId, Is.EqualTo(100010));
    }

    [Test]
    public async Task IncrementOpenCount_creates_then_updates()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IPackRepository>();
            await repo.IncrementOpenCount(viewerId, 10001, 3);
            await repo.IncrementOpenCount(viewerId, 10001, 2);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.PackOpenCounts).FirstAsync(x => x.Id == viewerId);
            var row = v.PackOpenCounts.Single(p => p.PackId == 10001);
            Assert.That(row.OpenCount, Is.EqualTo(5));
        }
    }

}
