using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

/// <summary>
/// Covers <see cref="GameConfigService"/>'s tier chain (DB → IConfiguration → ShippedDefaults →
/// <c>new T()</c>) and the atomic-per-section policy. Uses a real test SVSimDbContext from
/// <see cref="SVSimTestFactory"/> for the DB tier and an in-memory IConfiguration for the
/// appsettings tier. Test-only section types live in this file (assembly not scanned by the
/// seeder) so the fallback tiers can be exercised without fighting EnsureSeedDataAsync.
/// </summary>
public class GameConfigServiceTests
{
    // Real section type (in Models.Config, seeded by EnsureSeedDataAsync) — used to test DB and
    // override-DB scenarios.
    private const string PackRatesKey = "PackRates";

    // Test-only section types: not in SVSim.Database assembly → seeder ignores them → DB row is
    // never written by the seed step. Exercises appsettings / ShippedDefaults / new T() tiers
    // without having to delete seeded rows.
    [ConfigSection("UnseededWithFactory")]
    public class UnseededWithFactory
    {
        public string Value { get; set; } = "";
        public static UnseededWithFactory ShippedDefaults() => new() { Value = "from-shipped-defaults" };
    }

    [ConfigSection("UnseededNoFactory")]
    public class UnseededNoFactory
    {
        public int N { get; set; }
        // Intentionally no ShippedDefaults() — exercises the final `new T()` tier.
    }

    public class UnattributedSection
    {
        public string Foo { get; set; } = "";
    }

    private static IConfiguration EmptyConfig() =>
        new ConfigurationBuilder().Build();

    private static IConfiguration ConfigFrom(params (string key, string value)[] entries) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(entries.Select(e => new KeyValuePair<string, string?>(e.key, e.value)))
            .Build();

    [Test]
    public void Get_returns_DB_row_when_section_exists()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, EmptyConfig());

        // The fresh-install seeder wrote PackRates → tier 1 must hit it.
        var rates = svc.Get<PackRateConfig>();
        Assert.That(rates.AnimatedRate, Is.EqualTo(0.08).Within(1e-9),
            "tier-1 (DB) should return the seeded PackRates row");
        Assert.That(rates.PerSlot.Any(s => s.Slot == "8"), Is.True);
    }

    [Test]
    public void Get_atomic_DB_wins_even_when_appsettings_also_supplies_the_section()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        // Mutate DB row so we can detect which tier won.
        var row = db.GameConfigs.First(s => s.SectionName == PackRatesKey);
        var rates = JsonSerializer.Deserialize<PackRateConfig>(row.ValueJson)!;
        rates.AnimatedRate = 0.5;
        row.ValueJson = JsonSerializer.Serialize(rates);
        db.SaveChanges();

        // appsettings also supplies a different value — DB must still win (atomic per section).
        var appsettings = ConfigFrom(($"GameConfig:{PackRatesKey}:AnimatedRate", "0.99"));
        var svc = new GameConfigService(db, appsettings);

        var result = svc.Get<PackRateConfig>();
        Assert.That(result.AnimatedRate, Is.EqualTo(0.5).Within(1e-9),
            "atomic-per-section: DB row wins entirely; appsettings tier never consulted");
    }

    [Test]
    public void Get_falls_through_to_appsettings_when_no_DB_row()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var appsettings = ConfigFrom(("GameConfig:UnseededWithFactory:Value", "from-appsettings"));
        var svc = new GameConfigService(db, appsettings);

        var result = svc.Get<UnseededWithFactory>();
        Assert.That(result.Value, Is.EqualTo("from-appsettings"),
            "tier 2 should win when DB has no row and appsettings has the section");
    }

    [Test]
    public void Get_falls_through_to_ShippedDefaults_when_no_DB_row_and_no_appsettings_section()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, EmptyConfig());

        var result = svc.Get<UnseededWithFactory>();
        Assert.That(result.Value, Is.EqualTo("from-shipped-defaults"),
            "tier 3 (ShippedDefaults) should win when neither DB nor appsettings supplies the section");
    }

    [Test]
    public void Get_falls_through_to_parameterless_ctor_when_section_has_no_ShippedDefaults()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, EmptyConfig());

        var result = svc.Get<UnseededNoFactory>();
        Assert.That(result.N, Is.EqualTo(0),
            "tier 4 (new T()) should win when no other tier and no ShippedDefaults method exists");
    }

    [Test]
    public void Get_throws_when_section_type_is_not_marked_with_ConfigSection()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, EmptyConfig());

        var ex = Assert.Throws<InvalidOperationException>(() => svc.Get<UnattributedSection>());
        Assert.That(ex!.Message, Does.Contain("[ConfigSection"),
            "unmarked type must produce a clear diagnostic, not a silent fallback");
    }
}
