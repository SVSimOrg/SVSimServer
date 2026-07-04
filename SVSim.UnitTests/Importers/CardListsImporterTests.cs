using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

/// <summary>
/// Coverage for CardListsImporter (Stage 9C): one happy-path test per card-list sub-table plus
/// idempotency and orphan-warning behavior. Production seeds reference cards that don't exist in
/// the minimal 3-card test set, so the importer must complete without failing on FK orphans.
/// </summary>
public class CardListsImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task ImportAsync_writes_spot_cards_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        var rows = await db.SpotCards.ToListAsync();
        Assert.That(rows.Count, Is.GreaterThan(0), "spot-cards.json must produce rows");
        Assert.That(rows.All(r => r.Cost >= 0), Is.True, "Cost must be >= 0");
    }

    [Test]
    public async Task ImportAsync_writes_reprinted_cards_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.ReprintedCards.CountAsync(), Is.GreaterThan(0),
            "reprinted-cards.json must produce rows");
    }

    [Test]
    public async Task ImportAsync_writes_unlimited_restrictions_with_values()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        var rows = await db.UnlimitedRestrictions.ToListAsync();
        Assert.That(rows.Count, Is.GreaterThan(0), "unlimited-restrictions.json must produce rows");
        // RestrictionValue field must survive the import (e.g. 0 or 1).
        Assert.That(rows.All(r => r.RestrictionValue >= 0), Is.True);
    }

    [Test]
    public async Task ImportAsync_writes_loading_exclusion_cards_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.LoadingExclusionCards.CountAsync(), Is.GreaterThan(0),
            "loading-exclusion-cards.json must produce rows");
    }

    [Test]
    public async Task ImportAsync_handles_empty_maintenance_card_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // The shipped maintenance-cards.json is `[]` — confirm no rows created and no crash.
        await new CardListsImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.MaintenanceCards.CountAsync(), Is.EqualTo(0),
            "Empty maintenance seed should leave the table empty");
    }

    [Test]
    public async Task ImportAsync_handles_empty_feature_maintenance_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.FeatureMaintenances.CountAsync(), Is.EqualTo(0),
            "Empty feature-maintenances seed should leave the table empty");
    }

    [Test]
    public async Task ImportAsync_is_idempotent()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new CardListsImporter().ImportAsync(db, SeedDir);
        int spots1 = await db.SpotCards.CountAsync();
        int reprinted1 = await db.ReprintedCards.CountAsync();
        int unlimited1 = await db.UnlimitedRestrictions.CountAsync();
        int excl1 = await db.LoadingExclusionCards.CountAsync();

        await new CardListsImporter().ImportAsync(db, SeedDir);

        int spots2 = await db.SpotCards.CountAsync();
        int reprinted2 = await db.ReprintedCards.CountAsync();
        int unlimited2 = await db.UnlimitedRestrictions.CountAsync();
        int excl2 = await db.LoadingExclusionCards.CountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(spots2, Is.EqualTo(spots1));
            Assert.That(reprinted2, Is.EqualTo(reprinted1));
            Assert.That(unlimited2, Is.EqualTo(unlimited1));
            Assert.That(excl2, Is.EqualTo(excl1));
        });
    }

    [Test]
    public async Task ImportAsync_completes_when_seed_card_ids_are_orphans()
    {
        // The shipped seeds reference card_ids that DON'T exist in SVSimTestFactory's minimal
        // 3-card set — the orphan-warning path should log to stderr without throwing.
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        Assert.That(await db.Cards.CountAsync(), Is.EqualTo(3),
            "Test factory should seed exactly 3 cards (orphan-warning precondition)");

        Assert.DoesNotThrowAsync(async () =>
        {
            await new CardListsImporter().ImportAsync(db, SeedDir);
        });

        // Importer still wrote rows despite orphans.
        Assert.That(await db.SpotCards.CountAsync(), Is.GreaterThan(0));
    }

    [Test]
    public async Task ImportAsync_writes_feature_maintenances_from_tiny_fixture()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Build a temp seed dir with just feature-maintenances.json populated so we can exercise
        // the FeatureMaintenances clear-and-rewrite path without polluting the shipped seeds.
        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "feature-maintenances.json"),
                "[{\"id\":1,\"feature_key\":\"test_feature\",\"data\":{\"foo\":\"bar\"}}]");

            await new CardListsImporter().ImportAsync(db, tmp);

            var rows = await db.FeatureMaintenances.ToListAsync();
            Assert.That(rows.Count, Is.EqualTo(1));
            Assert.That(rows[0].FeatureKey, Is.EqualTo("test_feature"));
            Assert.That(rows[0].Data, Does.Contain("foo"));

            // Rerun: clear-and-rewrite should keep the table at 1 row (same data).
            await new CardListsImporter().ImportAsync(db, tmp);
            Assert.That(await db.FeatureMaintenances.CountAsync(), Is.EqualTo(1));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
