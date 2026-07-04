using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PaymentItemImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_items_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PaymentItemImporter().ImportAsync(db, SeedDir);

        var items = await db.PaymentItems.OrderBy(p => p.Id).ToListAsync();
        Assert.That(items.Count, Is.GreaterThan(0), "seed file must contain items");
        Assert.That(items.All(i => i.Price >= 0m), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PaymentItemImporter().ImportAsync(db, SeedDir);
        int before = await db.PaymentItems.CountAsync();
        await new PaymentItemImporter().ImportAsync(db, SeedDir);
        int after = await db.PaymentItems.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 99999;
        db.PaymentItems.Add(new SVSim.Database.Models.PaymentItemEntry
        {
            Id = legacyId,
            ProductId = 0,
            Name = "legacy",
            Price = 0m,
        });
        await db.SaveChangesAsync();

        await new PaymentItemImporter().ImportAsync(db, SeedDir);

        var legacy = await db.PaymentItems.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.Name, Is.EqualTo("legacy"));
    }

    [Test]
    public async Task Skips_rows_with_zero_record_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "payment-items.json"),
                "[{\"record_id\":0,\"product_id\":1,\"name\":\"junk\",\"price\":\"0\"}]");

            await new PaymentItemImporter().ImportAsync(db, tmp);

            int count = await db.PaymentItems.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with record_id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }
}
