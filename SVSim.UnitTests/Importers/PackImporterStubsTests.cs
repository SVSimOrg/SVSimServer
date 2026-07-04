using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PackImporterStubsTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    [Test]
    public async Task Live_capture_overrides_stub_on_conflict()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackImporter().ImportAsync(db, SeedDir);

        // 10001 is in both packs.json (no is_enabled -> defaults true) and pack-stubs.json
        // (is_enabled=false). Live capture wins -> IsEnabled stays true and gacha_detail
        // is the packs.json value, not "STUB CLC".
        var live = await db.Packs.FirstAsync(p => p.Id == 10001);
        Assert.That(live.IsEnabled, Is.True);
        Assert.That(live.GachaDetail, Does.Not.Contain("STUB"));
    }

    [Test]
    public async Task Stub_only_packs_are_inserted_with_IsEnabled_false()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await new PackImporter().ImportAsync(db, SeedDir);

        // 95001 is stub-only -> inserted with IsEnabled=false and the stub's gacha_detail
        // (the pack's short_code from cardsetnametext.json, plumbed through by the extractor).
        var stub = await db.Packs.FirstAsync(p => p.Id == 95001);
        Assert.That(stub.IsEnabled, Is.False);
        Assert.That(stub.GachaDetail, Is.EqualTo("7th"));
    }
}
