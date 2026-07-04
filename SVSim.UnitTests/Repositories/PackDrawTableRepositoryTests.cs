using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Repositories.PackDrawTables;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class PackDrawTableRepositoryTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task GetAsync_returns_null_when_pack_unseeded()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPackDrawTableRepository>();

        var table = await repo.GetAsync(123456);

        Assert.That(table, Is.Null);
    }

    [Test]
    public async Task GetAsync_returns_config_slot_rates_and_card_weights_for_seeded_pack()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IPackDrawTableRepository>();

        await new PackDrawTableImporter().ImportAsync(db, SeedDir);
        var table = await repo.GetAsync(10000);

        Assert.That(table, Is.Not.Null);
        Assert.That(table!.Config.AnimationRatePct, Is.EqualTo(8.0));
        Assert.That(table.SlotRates.Count, Is.GreaterThanOrEqualTo(4));   // bronze/silver/gold/legendary at minimum
        Assert.That(table.CardWeights.Count, Is.GreaterThan(0));
        Assert.That(table.CardWeights.All(w => w.PackId == 10000), Is.True);
    }
}
