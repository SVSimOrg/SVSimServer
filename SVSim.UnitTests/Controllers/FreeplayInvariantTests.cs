using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class FreeplayInvariantTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    [Test]
    public async Task Freeplay_pack_open_leaves_viewer_currency_unchanged()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCrystalPack(factory, viewerId);
        await factory.EnableFreeplayAsync();

        ulong before;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            before = (await db.Viewers.FirstAsync(v => v.Id == viewerId)).Currency.Crystals;
        }

        // Verify the precondition: viewer has 0 crystals, so without freeplay this would be rejected.
        Assert.That(before, Is.EqualTo(0UL), "precondition: viewer must be broke before the open");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // gacha_type:1 is the parent pack's gacha_type — see project_wire_pack_gacha_type memory.
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","parent_gacha_id":10001,"gacha_id":100002,"gacha_type":1,"pack_number":1,"exclude_card_ids":[]}""";
        var resp = await client.PostAsync("/pack/open", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        ulong after;
        using (var scope2 = factory.Services.CreateScope())
        {
            var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
            after = (await db2.Viewers.FirstAsync(v => v.Id == viewerId)).Currency.Crystals;
        }

        Assert.That(after, Is.EqualTo(before), "freeplay must not write currency to the DB");
    }

    /// <summary>
    /// Seeds a crystal pack (parent gacha 10001, child gacha_id 100002, TypeDetail = CardPackType.CrystalMulti, cost=100)
    /// with the viewer broke (0 crystals). Mirrors the pack shape from
    /// PackControllerOpenTests.Open_with_crystals_deducts_crystals — the only difference is
    /// Crystals=0 instead of 250, so without freeplay this open would be refused.
    /// </summary>
    private static async Task SeedCrystalPack(SVSimTestFactory f, long viewerId)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        int baseId = await db.CardSets.Where(s => s.Cards.Count > 0).Select(s => s.Id).FirstAsync();
        db.Packs.Add(new PackConfigEntry
        {
            Id = 10001, BasePackId = baseId, PackCategory = PackCategory.None,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaType = 1, GachaDetail = "test",
            ChildGachas = { new PackChildGachaEntry { GachaId = 100002, TypeDetail = CardPackType.CrystalMulti, Cost = 100, CardCount = 8 } },
        });
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 0;
        await db.SaveChangesAsync();
    }
}
