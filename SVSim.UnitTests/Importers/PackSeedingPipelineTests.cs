using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class PackSeedingPipelineTests
{
    [Test]
    public async Task SeedGlobals_loads_pack_catalog_from_fixture()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();   // uses test-fixture seed overlay copied into the test output dir (see SVSim.UnitTests.csproj)

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var packs = await db.Packs.OrderBy(p => p.Id).ToListAsync();

        Assert.That(packs.Count, Is.GreaterThanOrEqualTo(3), "fixture has at least 3 packs");
        var p10001 = packs.Single(p => p.Id == 10001);
        Assert.That(p10001.PackCategory, Is.EqualTo(PackCategory.None));
        Assert.That(p10001.BasePackId, Is.EqualTo(10001));
        Assert.That(p10001.SleeveId, Is.EqualTo(3000011));
        Assert.That(p10001.GachaPointConfig, Is.Not.Null);
        Assert.That(p10001.GachaPointConfig!.ExchangeablePoint, Is.EqualTo(400));
    }

    [Test]
    public async Task SeedGlobals_persists_child_gachas_with_correct_types_and_costs()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var pack = await db.Packs.AsNoTracking()
            .FirstAsync(p => p.Id == 10001);
        var children = pack.ChildGachas.OrderBy(c => c.GachaId).ToList();

        Assert.That(children.Count, Is.EqualTo(3));
        Assert.That(children.Select(c => c.TypeDetail),
            Is.EqualTo(new[] { CardPackType.CrystalMulti, CardPackType.Daily, CardPackType.RupyMulti }));
        Assert.That(children.Select(c => c.Cost), Is.EqualTo(new[] { 100, 50, 100 }));
        Assert.That(children.Single(c => c.TypeDetail == CardPackType.Daily).IsDailySingle, Is.True);
    }

    [Test]
    public async Task SeedGlobals_is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        await factory.SeedGlobalsAsync();   // second run must not duplicate or stack child gachas

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var pack = await db.Packs.AsNoTracking().FirstAsync(p => p.Id == 10001);
        Assert.That(pack.ChildGachas.Count, Is.EqualTo(3),
            "child_gacha_info is owned — rerun must replace, not stack.");
    }

    [Test]
    public async Task SeedGlobals_preserves_daily_free_gacha_count_on_free_child()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var pack = await db.Packs.AsNoTracking().FirstAsync(p => p.Id == 80032);
        var freeChild = pack.ChildGachas.Single(c => c.TypeDetail == CardPackType.FreePacks);

        Assert.That(freeChild.DailyFreeGachaCount, Is.EqualTo(1));
        Assert.That(freeChild.PurchaseLimitCount, Is.EqualTo(1));
        Assert.That(freeChild.FreeGachaCampaignId, Is.EqualTo(49));
        Assert.That(freeChild.CampaignName, Is.EqualTo("New Season Release Bonus"));
    }
}
