using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class ItemPurchaseImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_catalog_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new ItemPurchaseImporter().ImportAsync(db, SeedDir);

        var entries = await db.ItemPurchaseCatalog.OrderBy(e => e.Id).ToListAsync();
        Assert.That(entries.Count, Is.GreaterThan(0));

        // Spot-check purchase_id 1: One Time Only Seer's Globe — 5000 RedEther → 1 Item(1000),
        // lifetime quota of 1.
        var one = entries.First(e => e.Id == 1);
        Assert.That(one.RequireItemType, Is.EqualTo(1)); // RedEther
        Assert.That(one.RequireItemNum, Is.EqualTo(5000));
        Assert.That(one.PurchaseItemType, Is.EqualTo(4)); // Item
        Assert.That(one.PurchaseItemId, Is.EqualTo(1000)); // Seer's Globe
        Assert.That(one.IsMonthlyReset, Is.False);
        Assert.That(one.PurchaseLimit, Is.EqualTo(1));
        Assert.That(one.PurchaseName, Does.Contain("Seer's Globe"));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new ItemPurchaseImporter().ImportAsync(db, SeedDir);
        int before = await db.ItemPurchaseCatalog.CountAsync();
        await new ItemPurchaseImporter().ImportAsync(db, SeedDir);
        int after = await db.ItemPurchaseCatalog.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 999999;
        db.ItemPurchaseCatalog.Add(new ItemPurchaseCatalogEntry { Id = legacyId, PurchaseName = "legacy", IsEnabled = true });
        await db.SaveChangesAsync();

        await new ItemPurchaseImporter().ImportAsync(db, SeedDir);

        var legacy = await db.ItemPurchaseCatalog.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null);
        Assert.That(legacy!.PurchaseName, Is.EqualTo("legacy"));
    }
}
