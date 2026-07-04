using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PackDrawTableImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Imports_config_slot_rates_and_card_weights()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackDrawTableImporter().ImportAsync(db, SeedDir);

        // Production seed is the source of truth in test output (no test-fixture overlay).
        Assert.That(await db.PackDrawConfigs.CountAsync(), Is.GreaterThanOrEqualTo(200));
        Assert.That(await db.PackDrawSlotRates.CountAsync(), Is.GreaterThanOrEqualTo(1000));
        Assert.That(await db.PackDrawCardWeights.CountAsync(), Is.GreaterThanOrEqualTo(50_000));

        // 98001 is a Guaranteed-Leader-Card bundle — bonus slot must contain rate-less
        // Special-tier leader rows.
        var bonus = await db.PackDrawCardWeights
            .Where(w => w.PackId == 98001 && w.Slot == DrawSlot.Bonus)
            .ToListAsync();
        Assert.That(bonus.Count, Is.GreaterThan(0));
        Assert.That(bonus.All(w => w.RatePct == null && w.IsLeader && w.Tier == DrawTier.Special), Is.True);
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackDrawTableImporter().ImportAsync(db, SeedDir);
        int before = await db.PackDrawCardWeights.CountAsync();
        await new PackDrawTableImporter().ImportAsync(db, SeedDir);
        int after = await db.PackDrawCardWeights.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }
}
