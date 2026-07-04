using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class ItemImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_items_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new ItemImporter().ImportAsync(db, SeedDir);

        var items = await db.Items.OrderBy(i => i.Id).ToListAsync();
        Assert.That(items.Count, Is.GreaterThan(0), "seed file must contain items");
        // Spot-check the card-pack-ticket cluster: Type==2, thumbnail follows ticket_<id> convention.
        var pack = items.FirstOrDefault(i => i.Id == 10032);
        Assert.That(pack, Is.Not.Null, "card-pack ticket 10032 (latest expansion) should be seeded");
        Assert.That(pack!.Type, Is.EqualTo(2));
        Assert.That(pack.ThumbnailPath, Is.EqualTo("ticket_10032"));
        Assert.That(pack.Name, Is.Not.Empty, "name should resolve via itemtext");
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new ItemImporter().ImportAsync(db, SeedDir);
        int before = await db.Items.CountAsync();
        await new ItemImporter().ImportAsync(db, SeedDir);
        int after = await db.Items.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 99999;
        db.Items.Add(new SVSim.Database.Models.ItemEntry
        {
            Id = legacyId,
            Name = "legacy",
            Type = 0,
            ThumbnailPath = "",
        });
        await db.SaveChangesAsync();

        await new ItemImporter().ImportAsync(db, SeedDir);

        var legacy = await db.Items.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.Name, Is.EqualTo("legacy"));
    }

    [Test]
    public async Task Skips_rows_with_zero_item_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "items.json"),
                "[{\"item_id\":0,\"name\":\"junk\",\"type\":1,\"thumbnail_path\":\"\"}]");

            await new ItemImporter().ImportAsync(db, tmp);

            int count = await db.Items.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with item_id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }
}
