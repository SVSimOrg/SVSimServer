using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class MyPageGlobalsImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_banners_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportBannersAsync(db, SeedDir);

        int count = await db.Banners.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "seed must contain banners");
    }

    [Test]
    public async Task Imports_sealed_singleton()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportSealedAsync(db, SeedDir);

        var row = await db.SealedSeasons.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Id, Is.EqualTo(1));
        // pack_info is a JSON array column, must be non-empty in the captured seed.
        Assert.That(row.PackInfo, Does.StartWith("["));
    }

    [Test]
    public async Task Imports_master_point_ranking_period()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportMasterPointRankingPeriodAsync(db, SeedDir);

        int count = await db.MasterPointRankingPeriods.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "seed must contain at least one ranking period");
    }

    [Test]
    public async Task Imports_special_deck_formats()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportSpecialDeckFormatsAsync(db, SeedDir);

        int count = await db.SpecialDeckFormats.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "seed must contain special deck formats");
    }

    [Test]
    public async Task Banners_are_clear_and_rewrite()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Pre-seed a legacy banner with Id outside the seed range — the importer must wipe it.
        db.Banners.Add(new BannerEntry
        {
            Id = 999,
            ImageName = "legacy_banner",
            Click = "legacy",
            Status = "9",
            ChangeTime = 0,
            RemainingTime = 0,
            ImagePaths = "[]",
        });
        await db.SaveChangesAsync();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportBannersAsync(db, SeedDir);

        var stale = await db.Banners.FindAsync(999);
        Assert.That(stale, Is.Null,
            "ImportBannersAsync must clear-and-rewrite — pre-existing legacy rows must be removed");
    }

    [Test]
    public async Task Special_deck_formats_are_clear_and_rewrite()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.SpecialDeckFormats.Add(new SpecialDeckFormatEntry
        {
            Id = 999,
            DeckFormat = "99",
            EndTime = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportSpecialDeckFormatsAsync(db, SeedDir);

        var stale = await db.SpecialDeckFormats.FindAsync(999);
        Assert.That(stale, Is.Null,
            "ImportSpecialDeckFormatsAsync must clear-and-rewrite — pre-existing legacy rows must be removed");
    }

    [Test]
    public async Task Singletons_are_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportSealedAsync(db, SeedDir);

        await importer.ImportSealedAsync(db, SeedDir);

        Assert.That(await db.SealedSeasons.CountAsync(), Is.EqualTo(1),
            "SealedSeason singleton must remain a single row on re-run");
    }

    [Test]
    public async Task Imports_home_dialogs_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportHomeDialogsAsync(db, SeedDir);

        var rows = await db.HomeDialogEntries.OrderBy(e => e.Id).ToListAsync();
        Assert.That(rows.Count, Is.GreaterThan(0), "seed must contain at least one home dialog");

        var first = rows[0];
        Assert.That(first.TitleTextId, Is.Not.Empty);
        Assert.That(first.Image, Is.Not.Empty);
        Assert.That(first.ButtonListJson, Does.StartWith("["),
            "button_list must round-trip as a JSON array string");
        Assert.That(first.EndTime, Is.GreaterThan(first.BeginTime));
    }

    [Test]
    public async Task Home_dialogs_are_clear_and_rewrite()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.HomeDialogEntries.Add(new HomeDialogEntry
        {
            Id = 999,
            TitleTextId = "legacy",
            Image = "legacy_image",
            ButtonListJson = "[]",
            BeginTime = DateTime.UtcNow.AddYears(-1),
            EndTime = DateTime.UtcNow.AddYears(-1).AddDays(1),
        });
        await db.SaveChangesAsync();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportHomeDialogsAsync(db, SeedDir);

        var stale = await db.HomeDialogEntries.FindAsync(999);
        Assert.That(stale, Is.Null,
            "ImportHomeDialogsAsync must clear-and-rewrite — pre-existing legacy rows must be removed");
    }

    [Test]
    public async Task Master_point_ranking_period_leaves_existing_rows_untouched()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        const int legacyId = 88888;
        db.MasterPointRankingPeriods.Add(new MasterPointRankingPeriodEntry
        {
            Id = legacyId,
            PeriodNum = 87,
            NecessaryScore = 12345,
            BeginTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        await db.SaveChangesAsync();

        var importer = new MyPageGlobalsImporter();
        await importer.ImportMasterPointRankingPeriodAsync(db, SeedDir);

        var legacy = await db.MasterPointRankingPeriods.FindAsync(legacyId);
        Assert.That(legacy, Is.Not.Null,
            "MasterPointRankingPeriod upserts by id — legacy rows not in seed must survive");
        Assert.That(legacy!.PeriodNum, Is.EqualTo(87));
        Assert.That(legacy.NecessaryScore, Is.EqualTo(12345));
    }
}
