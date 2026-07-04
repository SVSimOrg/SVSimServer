using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class SpotCardExchangeImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_catalog_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new SpotCardExchangeImporter().ImportAsync(db, SeedDir);

        var entries = await db.SpotCardExchangeCatalog.ToListAsync();
        Assert.That(entries.Count, Is.GreaterThan(0));

        // Spot-check: card 113041010 (class 0, exchange_point 3500, ts_rotation_id 10013)
        var c = entries.FirstOrDefault(e => e.Id == 113041010);
        Assert.That(c, Is.Not.Null);
        Assert.That(c!.ClassId, Is.EqualTo(0));
        Assert.That(c.ExchangePoint, Is.EqualTo(3500));
        Assert.That(c.TsRotationId, Is.EqualTo(10013));
        Assert.That(c.IsPreRelease, Is.False);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new SpotCardExchangeImporter().ImportAsync(db, SeedDir);
        int before = await db.SpotCardExchangeCatalog.CountAsync();
        await new SpotCardExchangeImporter().ImportAsync(db, SeedDir);
        int after = await db.SpotCardExchangeCatalog.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const long legacyId = 999_999_999L;
        db.SpotCardExchangeCatalog.Add(new SpotCardExchangeEntry
        {
            Id = legacyId, ClassId = 9, ExchangePoint = 99999, TsRotationId = 1, IsEnabled = true,
        });
        await db.SaveChangesAsync();

        await new SpotCardExchangeImporter().ImportAsync(db, SeedDir);

        var legacy = await db.SpotCardExchangeCatalog.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null);
        Assert.That(legacy!.ExchangePoint, Is.EqualTo(99999));
    }
}
