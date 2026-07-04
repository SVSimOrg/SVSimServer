using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

/// <summary>
/// Happy-path coverage for the load-index importer classes introduced in Stage 9B
/// (RotationConfig, MyRotation, AvatarAbility, ArenaSeason,
/// PreReleaseInfo). Each test instantiates the importer in isolation and verifies it inserts
/// rows from the corresponding seed file under <c>Data/seeds/</c>.
/// Idempotency, edge cases, and per-importer detail tests live in dedicated *ImporterTests files (e.g. BattlePassImporterTests).
/// </summary>
public class LoadIndexImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task RotationConfigImporter_writes_game_config_sections()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new RotationConfigImporter().ImportAsync(db, SeedDir);

        var rows = await db.GameConfigs.ToListAsync();
        Assert.That(rows.Any(r => r.SectionName == "Rotation"), Is.True,
            "Rotation section must be written from rotation-config.json");
    }

    [Test]
    public async Task MyRotationImporter_writes_settings_and_abilities()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new MyRotationImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.MyRotationSettings.CountAsync(), Is.GreaterThan(0),
            "my-rotation-settings.json must produce setting rows");
        Assert.That(await db.MyRotationAbilities.CountAsync(), Is.GreaterThan(0),
            "my-rotation-abilities.json must produce ability rows");
    }

    [Test]
    public async Task AvatarAbilityImporter_writes_abilities()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new AvatarAbilityImporter().ImportAsync(db, SeedDir);

        Assert.That(await db.AvatarAbilities.CountAsync(), Is.GreaterThan(0),
            "avatar-abilities.json must produce ability rows");
    }

    [Test]
    public async Task ArenaSeasonImporter_writes_singleton()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new ArenaSeasonImporter().ImportAsync(db, SeedDir);

        var row = await db.ArenaSeasons.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.That(row, Is.Not.Null, "ArenaSeason singleton id=1 must be written");
        Assert.That(row!.FormatInfo, Is.Not.EqualTo("{}"), "format_info blob must be populated from seed");
    }

    [Test]
    public async Task PreReleaseInfoImporter_writes_singleton()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PreReleaseInfoImporter().ImportAsync(db, SeedDir);

        var row = await db.PreReleaseInfos.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.That(row, Is.Not.Null, "PreReleaseInfo singleton id=1 must be written");
        Assert.That(row!.PreReleaseId, Is.Not.Empty, "pre_release_id field must be populated");
    }
}
