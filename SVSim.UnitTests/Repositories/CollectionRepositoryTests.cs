using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Collectibles;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class CollectionRepositoryTests
{
    [Test]
    public async Task GetAllSleeveIds_returns_every_master_sleeve()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.Sleeves.Add(new SleeveEntry { Id = 123456 });
        await db.SaveChangesAsync();

        var repo = new CollectionRepository(db);
        var ids = await repo.GetAllSleeveIds();

        Assert.That(ids, Does.Contain(123456));
        Assert.That(ids.Count, Is.EqualTo(await db.Sleeves.CountAsync()));
    }
}
