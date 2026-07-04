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

public class PackControllerInfoTests
{
    private const string EmptyEnvelope = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task SeedActivePack(SVSimTestFactory f, int parentId, int baseId, PackCategory cat)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.Packs.Add(new PackConfigEntry
        {
            Id = parentId, BasePackId = baseId, PackCategory = cat,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaType = 1, GachaDetail = "test",
            ChildGachas = { new PackChildGachaEntry { GachaId = parentId * 10 + 7, TypeDetail = CardPackType.RupyMulti, Cost = 100, CardCount = 8 } },
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Info_returns_active_packs_only()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedActivePack(factory, 10001, 10001, PackCategory.None);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var list = doc.RootElement.GetProperty("pack_config_list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1));
        Assert.That(list[0].GetProperty("parent_gacha_id").GetInt32(), Is.EqualTo(10001));
    }

    [Test]
    public async Task Info_overlays_viewer_open_count()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedActivePack(factory, 10001, 10001, PackCategory.None);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.PackOpenCounts).FirstAsync(x => x.Id == viewerId);
            v.PackOpenCounts.Add(new ViewerPackOpenCount { PackId = 10001, OpenCount = 7 });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var p = doc.RootElement.GetProperty("pack_config_list")[0];
        Assert.That(p.GetProperty("open_count").GetInt32(), Is.EqualTo(7));
    }

    [Test]
    public async Task Info_emits_child_gacha_info_with_correct_wire_keys()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedActivePack(factory, 10001, 10001, PackCategory.None);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var children = doc.RootElement.GetProperty("pack_config_list")[0].GetProperty("child_gacha_info");
        Assert.That(children.GetArrayLength(), Is.EqualTo(1));
        Assert.That(children[0].GetProperty("type_detail").GetInt32(), Is.EqualTo(7));
        Assert.That(children[0].GetProperty("cost").GetInt32(), Is.EqualTo(100));
    }

    [Test]
    public async Task Info_emits_gacha_point_key_as_null_when_pack_has_no_gacha_point_config()
    {
        // PackInfoTask.cs:126 does `if (jsonData2["gacha_point"] != null)`. LitJson's JsonData
        // indexer throws KeyNotFoundException on missing keys — the null check protects against
        // null *value*, not missing *key*. With Program.cs's global WhenWritingNull, a null
        // PackGachaPointDto would be omitted entirely and crash the client. Override per
        // [[project_wire_null_policy]].
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Seed a pack WITHOUT GachaPointConfig — matches packs 80047, 92001, 99047 in prod
        // (legendary specials whose `gacha_point` is null).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 92001, BasePackId = 90001, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "legendary special", SleeveId = 5090001,
                GachaPointConfig = null,
                ChildGachas = { new PackChildGachaEntry { GachaId = 920002, TypeDetail = CardPackType.TicketMulti, Cost = 1, CardCount = 8, ItemId = 92001 } },
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var pack = doc.RootElement.GetProperty("pack_config_list")[0];

        // The key MUST be present, even though its value is null.
        Assert.That(pack.TryGetProperty("gacha_point", out var gachaPoint), Is.True,
            "gacha_point key must always be present in /pack/info — client at PackInfoTask.cs:126 does a direct key access guarded only by a null check, not Keys.Contains.");
        Assert.That(gachaPoint.ValueKind, Is.EqualTo(JsonValueKind.Null),
            "gacha_point should serialize as explicit null when no GachaPointConfig is set.");
    }

    [Test]
    public async Task Info_projects_viewer_gacha_point_balance_into_gacha_point_block()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "test",
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
                ChildGachas =
                {
                    // Must include at least one non-ticket child so this pack is NOT ticket-only
                    // and remains visible with a gacha_point block.
                    new PackChildGachaEntry { GachaId = 100087, TypeDetail = CardPackType.RupyMulti, Cost = 100, CardCount = 8 },
                },
            });
            var viewer = await db.Viewers
                .Include(v => v.GachaPointBalances)
                .FirstAsync(v => v.Id == viewerId);
            viewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 450 });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var pack = doc.RootElement.GetProperty("pack_config_list")[0];
        var gp = pack.GetProperty("gacha_point");
        Assert.That(gp.GetProperty("gacha_point").GetInt32(), Is.EqualTo(450));
        Assert.That(gp.GetProperty("is_exchangeable_gacha_point").GetBoolean(), Is.True,
            "balance >= threshold should flip the gate");
    }

    [Test]
    public async Task Info_omits_gacha_point_block_for_ticket_only_packs()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            // Even though the pack has a GachaPointConfig, it must be hidden because every
            // child is a ticket type (4 or 5). Mirrors prod for starter pack 99047.
            db.Packs.Add(new PackConfigEntry
            {
                Id = 99047, BasePackId = 99047, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "test",
                GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
                ChildGachas =
                {
                    new PackChildGachaEntry { GachaId = 990475, TypeDetail = CardPackType.TicketMulti, Cost = 0, CardCount = 8 },
                },
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);

        using var doc = JsonDocument.Parse(text);
        var pack = doc.RootElement.GetProperty("pack_config_list")[0];
        // Either the key is absent (WhenWritingNull dropped it) or the value is null.
        if (pack.TryGetProperty("gacha_point", out var gp))
        {
            Assert.That(gp.ValueKind, Is.EqualTo(JsonValueKind.Null),
                "ticket-only pack must not emit a gacha_point block");
        }
    }

    [Test]
    public async Task Info_includes_free_pack_child_when_no_claim_today()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 80032, BasePackId = 80001, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "throwback test", SleeveId = 5090001,
                ChildGachas =
                {
                    new PackChildGachaEntry { GachaId = 800032, TypeDetail = CardPackType.TicketMulti, Cost = 1, CardCount = 8, ItemId = 80001 },
                    new PackChildGachaEntry
                    {
                        GachaId = 780032, TypeDetail = CardPackType.FreePacks, Cost = 1, CardCount = 8,
                        PurchaseLimitCount = 1, DailyFreeGachaCount = 1,
                        FreeGachaCampaignId = 49, CampaignName = "Test Campaign",
                    },
                },
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var pack = doc.RootElement.GetProperty("pack_config_list").EnumerateArray()
            .Single(p => p.GetProperty("parent_gacha_id").GetInt32() == 80032);
        var children = pack.GetProperty("child_gacha_info").EnumerateArray().ToList();
        Assert.That(children.Count, Is.EqualTo(2), "free + ticket children both visible pre-claim");
        var free = children.Single(c => c.GetProperty("type_detail").GetInt32() == 10);
        Assert.That(free.GetProperty("free_gacha_campaign_id").GetInt32(), Is.EqualTo(49));
        Assert.That(free.GetProperty("campaign_name").GetString(), Is.EqualTo("Test Campaign"));
        Assert.That(free.GetProperty("daily_free_gacha_count").GetString(), Is.EqualTo("1"));
        Assert.That(free.GetProperty("purchase_limit_count").GetString(), Is.EqualTo("1"));
    }

    [Test]
    public async Task Info_drops_free_pack_child_when_claimed_today()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 80033, BasePackId = 80001, PackCategory = PackCategory.LegendCardPack,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "throwback test", SleeveId = 5090001,
                ChildGachas =
                {
                    new PackChildGachaEntry { GachaId = 800033, TypeDetail = CardPackType.TicketMulti, Cost = 1, CardCount = 8, ItemId = 80001 },
                    new PackChildGachaEntry
                    {
                        GachaId = 780033, TypeDetail = CardPackType.FreePacks, Cost = 1, CardCount = 8,
                        DailyFreeGachaCount = 1, FreeGachaCampaignId = 50, CampaignName = "X",
                    },
                },
            });
            var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
            v.FreePackClaims.Add(new ViewerFreePackClaim
            {
                FreeGachaCampaignId = 50, ClaimCount = 1, LastClaimedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var pack = doc.RootElement.GetProperty("pack_config_list").EnumerateArray()
            .Single(p => p.GetProperty("parent_gacha_id").GetInt32() == 80033);
        var children = pack.GetProperty("child_gacha_info").EnumerateArray().ToList();
        Assert.That(children.Count, Is.EqualTo(1), "Only the ticket child should remain after today's free claim");
        Assert.That(children[0].GetProperty("type_detail").GetInt32(), Is.EqualTo(5));
    }

    [Test]
    public async Task Info_emits_is_daily_single_true_before_todays_claim()
    {
        // Client's DAILY-branch UI (GachaPackAreaLayout.cs:420) renders the half-off button
        // iff the wire ships `is_daily_single: true`. Pre-claim it must be truthy.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10001, BasePackId = 10001, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "daily test",
                ChildGachas =
                {
                    new PackChildGachaEntry { GachaId = 200001, TypeDetail = CardPackType.Daily, Cost = 50, CardCount = 8, IsDailySingle = true },
                    new PackChildGachaEntry { GachaId = 100002, TypeDetail = CardPackType.CrystalMulti, Cost = 100, CardCount = 8 },
                },
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var daily = doc.RootElement.GetProperty("pack_config_list")[0]
            .GetProperty("child_gacha_info").EnumerateArray()
            .Single(c => c.GetProperty("type_detail").GetInt32() == 3);
        Assert.That(daily.TryGetProperty("is_daily_single", out var flag), Is.True,
            "is_daily_single must be present when claimable");
        Assert.That(flag.GetBoolean(), Is.True);
    }

    [Test]
    public async Task Info_suppresses_is_daily_single_after_todays_claim()
    {
        // Bug repro: after a successful DAILY open, /pack/info kept emitting is_daily_single=true,
        // so the client re-showed the half-off button. Clicking it again 400'd with
        // daily_free_already_claimed. Post-claim the flag must be false (or omitted) so the
        // client's DAILY-branch renders the full-price crystal button instead. The child entry
        // itself stays so CRYSTAL_MULTI still activates.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Packs.Add(new PackConfigEntry
            {
                Id = 10001, BasePackId = 10001, PackCategory = PackCategory.None,
                CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
                GachaType = 1, GachaDetail = "daily test",
                ChildGachas =
                {
                    new PackChildGachaEntry { GachaId = 200001, TypeDetail = CardPackType.Daily, Cost = 50, CardCount = 8, IsDailySingle = true },
                    new PackChildGachaEntry { GachaId = 100002, TypeDetail = CardPackType.CrystalMulti, Cost = 100, CardCount = 8 },
                },
            });
            var v = await db.Viewers.Include(x => x.PackOpenCounts).FirstAsync(x => x.Id == viewerId);
            v.PackOpenCounts.Add(new ViewerPackOpenCount { PackId = 10001, OpenCount = 1, LastDailyFreeAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/pack/info", JsonBody(EmptyEnvelope));
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var children = doc.RootElement.GetProperty("pack_config_list")[0]
            .GetProperty("child_gacha_info").EnumerateArray().ToList();
        Assert.That(children.Count, Is.EqualTo(2), "child entries stay; only the daily-single flag flips");

        var daily = children.Single(c => c.GetProperty("type_detail").GetInt32() == 3);
        // With WhenWritingDefault, the wire omits the key entirely when false — either shape is
        // fine (PackInfoTask.cs:143-150 defaults to false on missing key).
        bool value = daily.TryGetProperty("is_daily_single", out var flag) && flag.GetBoolean();
        Assert.That(value, Is.False,
            "is_daily_single must be false/absent post-claim so the client stops offering the half-off button");
    }
}
