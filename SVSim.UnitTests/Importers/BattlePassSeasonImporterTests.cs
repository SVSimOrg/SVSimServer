using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BattlePassSeasonImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_season_from_seed_file()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassSeasonImporter().ImportAsync(db, SeedDir);

        var season = await db.BattlePassSeasons.SingleAsync(s => s.Id == 23);
        Assert.That(season.Name, Does.Contain("Season"));
        Assert.That(season.MaxLevel, Is.EqualTo(100));
        Assert.That(season.CanPurchase, Is.True);
        Assert.That(season.PriceCrystal, Is.EqualTo(980));
        // JST-offset seed is converted to UTC for Postgres 'timestamp with time zone' compatibility.
        // Semantically lossless — the instant 2026-04-01T02:00+09:00 == 2026-03-31T17:00 UTC.
        Assert.That(season.StartDate.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(season.StartDate.UtcDateTime, Is.EqualTo(new DateTime(2026, 3, 31, 17, 0, 0, DateTimeKind.Utc)));
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new BattlePassSeasonImporter().ImportAsync(db, SeedDir);
        int before = await db.BattlePassSeasons.CountAsync();
        await new BattlePassSeasonImporter().ImportAsync(db, SeedDir);
        int after = await db.BattlePassSeasons.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Updates_existing_row_when_seed_changes()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.BattlePassSeasons.Add(new SVSim.Database.Models.BattlePassSeasonEntry
        {
            Id = 23, Name = "stale", MaxLevel = 0, PriceCrystal = 0,
            StartDate = DateTimeOffset.MinValue, EndDate = DateTimeOffset.MinValue,
        });
        await db.SaveChangesAsync();

        await new BattlePassSeasonImporter().ImportAsync(db, SeedDir);

        var refreshed = await db.BattlePassSeasons.FindAsync(23);
        Assert.That(refreshed!.Name, Is.Not.EqualTo("stale"));
        Assert.That(refreshed.PriceCrystal, Is.EqualTo(980));
    }

    [Test]
    public async Task Empty_seed_is_no_op()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        string tmp = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid()}");
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "battle-pass-seasons.json"), "[]");
            await new BattlePassSeasonImporter().ImportAsync(db, tmp);
            int count = await db.BattlePassSeasons.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }
        finally { Directory.Delete(tmp, true); }
    }
}
