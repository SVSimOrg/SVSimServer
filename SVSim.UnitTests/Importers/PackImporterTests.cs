using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PackImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_packs_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackImporter().ImportAsync(db, SeedDir);

        var packs = await db.Packs.OrderBy(p => p.Id).ToListAsync();
        Assert.That(packs.Count, Is.GreaterThan(0), "seed file must contain packs");
        Assert.That(packs.All(p => p.Id > 0), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackImporter().ImportAsync(db, SeedDir);
        int before = await db.Packs.CountAsync();
        await new PackImporter().ImportAsync(db, SeedDir);
        int after = await db.Packs.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Leaves_existing_rows_untouched_when_missing_from_seed()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 99999;
        db.Packs.Add(new PackConfigEntry
        {
            Id = legacyId,
            BasePackId = legacyId,
            GachaType = 1,
            PackCategory = PackCategory.None,
            GachaDetail = "legacy",
        });
        await db.SaveChangesAsync();

        await new PackImporter().ImportAsync(db, SeedDir);

        var legacy = await db.Packs.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null, "seed-missing row must be left intact");
        Assert.That(legacy!.GachaDetail, Is.EqualTo("legacy"));
    }

    [Test]
    public async Task Skips_rows_with_zero_parent_gacha_id()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "packs.json"),
                "[{\"parent_gacha_id\":0,\"base_pack_id\":1,\"gacha_type\":1,\"pack_category\":0,\"child_gachas\":[],\"banners\":[]}]");

            await new PackImporter().ImportAsync(db, tmp);

            int count = await db.Packs.CountAsync();
            Assert.That(count, Is.EqualTo(0), "rows with parent_gacha_id=0 must not be inserted");
        }
        finally { Directory.Delete(tmp, true); }
    }

    [Test]
    public async Task Owned_collections_are_replaced_wholesale_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackImporter().ImportAsync(db, SeedDir);
        var pack1 = await db.Packs.AsNoTracking().FirstAsync(p => p.Id == 10001);
        int childCountBefore = pack1.ChildGachas.Count;
        int bannerCountBefore = pack1.Banners.Count;

        // Re-run: owned collections must NOT stack. Same fixture content -> same counts.
        await new PackImporter().ImportAsync(db, SeedDir);

        var pack2 = await db.Packs.AsNoTracking().FirstAsync(p => p.Id == 10001);
        Assert.That(pack2.ChildGachas.Count, Is.EqualTo(childCountBefore),
            "child_gachas must be replaced wholesale on rerun, not stacked");
        Assert.That(pack2.Banners.Count, Is.EqualTo(bannerCountBefore),
            "banners must be replaced wholesale on rerun, not stacked");
    }
}
