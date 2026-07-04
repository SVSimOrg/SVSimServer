using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class PackControllerSetRotationStarterClassTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds an active RotationStarterCardPack (id 93025) with a minimal class-1-only draw table
    /// sufficient to satisfy /pack/open after a class is locked in. The class-axis sampling in
    /// PackOpenService is covered by PackOpenServiceTests; this helper only needs a working table.
    /// Returns the pack id.
    /// </summary>
    private static async Task<int> SeedRotationStarterPack(SVSimTestFactory f, long viewerId, int packId = 93025, ulong rupees = 5000)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var set = await db.CardSets.Include(s => s.Cards).FirstAsync(s => s.Cards.Count > 0);
        int baseId = set.Id;
        long cardId = set.Cards.First().Id;

        db.Packs.Add(new PackConfigEntry
        {
            Id = packId,
            BasePackId = baseId,
            PackCategory = PackCategory.RotationStarterCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1),
            CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaType = 1,
            GachaDetail = "rs-test",
            SleeveId = 3000011,
            ChildGachas = {
                new PackChildGachaEntry { GachaId = packId * 10 + 1, TypeDetail = CardPackType.RupyMulti, Cost = 100, CardCount = 8 },
            },
        });

        db.PackDrawConfigs.Add(new PackDrawConfigEntry { Id = packId, AnimationRatePct = 0 });
        // Both slots: bronze 100% — single tier keeps the test deterministic.
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Bronze, RatePct = 100.0 });
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, RatePct = 100.0 });
        // Class 3 pool only — verifies the class filter actually narrows. Open requests for
        // any other class would fall through to FallbackAcrossTiers and 500 if the test ever
        // expected them to succeed.
        db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 3, CardId = cardId, RatePct = 100 });
        db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, ClassId = 3, CardId = cardId, RatePct = 100 });

        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Rupees = rupees;
        await db.SaveChangesAsync();
        return packId;
    }

    [Test]
    public async Task SetRotationStarterClass_persists_and_appears_in_pack_info()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var setJson = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":3}""";
        var setResp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(setJson));
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var infoResp = await client.PostAsync("/pack/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        Assert.That(infoResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var packs = doc.RootElement.GetProperty("pack_config_list");
        JsonElement? pack = null;
        foreach (var p in packs.EnumerateArray())
        {
            if (p.GetProperty("parent_gacha_id").GetInt32() == packId) { pack = p; break; }
        }
        Assert.That(pack, Is.Not.Null, $"pack {packId} should be present in /pack/info");
        Assert.That(pack!.Value.TryGetProperty("selected_class_id", out var sel), Is.True,
            "selected_class_id must be present on the parent PackConfig after the choice");
        Assert.That(sel.GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task SetRotationStarterClass_omits_selected_class_id_before_choice()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var infoResp = await client.PostAsync("/pack/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var body = await infoResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var pack = doc.RootElement.GetProperty("pack_config_list")
            .EnumerateArray().First(p => p.GetProperty("parent_gacha_id").GetInt32() == packId);
        // Global WhenWritingNull policy applies — key absent when no choice exists.
        Assert.That(pack.TryGetProperty("selected_class_id", out _), Is.False,
            "selected_class_id key must be absent before the viewer has chosen");
    }

    [Test]
    public async Task SetRotationStarterClass_rejects_invalid_class()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        foreach (int bad in new[] { 0, 9, -1, 100 })
        {
            var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":{{bad}}}""";
            var resp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(json));
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"class_id={bad} should be rejected");
        }
    }

    [Test]
    public async Task SetRotationStarterClass_rejects_already_chosen()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":3}""";
        var first = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(json));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json2 = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":5}""";
        var second = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(json2));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "second commit should be rejected — choice is one-shot per pack");
    }

    [Test]
    public async Task SetRotationStarterClass_rejects_non_rotation_starter_pack()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Seed a standard expansion pack (Category.None) — class lock doesn't apply.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            int baseId = await db.CardSets.Where(s => s.Cards.Count > 0).Select(s => s.Id).FirstAsync();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10001, BasePackId = baseId, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "ord", SleeveId = 3000011,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":10001,"class_id":3}""";
        var resp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SetRotationStarterClass_rejects_unknown_pack()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":99999999,"class_id":3}""";
        var resp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Open_rs_pack_without_prior_class_commit_is_rejected()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Skip /set_rotation_starter_class — go straight to /pack/open with class_id=3.
        var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","parent_gacha_id":{{packId}},"gacha_id":{{packId * 10 + 1}},"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"class_id":3}""";
        var resp = await client.PostAsync("/pack/open", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "/pack/open on an RS pack must require a prior /set_rotation_starter_class commit");
    }

    [Test]
    public async Task Open_rs_pack_rejects_class_id_mismatch()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Commit class 3, then attempt to open as class 5.
        var setJson = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":3}""";
        var setResp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(setJson));
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var openJson = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","parent_gacha_id":{{packId}},"gacha_id":{{packId * 10 + 1}},"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"class_id":5}""";
        var openResp = await client.PostAsync("/pack/open", JsonBody(openJson));
        Assert.That(openResp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "/pack/open class_id must match the persisted choice");
    }

    [Test]
    public async Task Open_rs_pack_with_matching_class_id_succeeds()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        int packId = await SeedRotationStarterPack(factory, viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var setJson = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","pack_id":{{packId}},"class_id":3}""";
        var setResp = await client.PostAsync("/pack/set_rotation_starter_class", JsonBody(setJson));
        Assert.That(setResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var openJson = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","parent_gacha_id":{{packId}},"gacha_id":{{packId * 10 + 1}},"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"class_id":3}""";
        var openResp = await client.PostAsync("/pack/open", JsonBody(openJson));
        Assert.That(openResp.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"/pack/open with matching class should succeed; body={await openResp.Content.ReadAsStringAsync()}");
    }
}
