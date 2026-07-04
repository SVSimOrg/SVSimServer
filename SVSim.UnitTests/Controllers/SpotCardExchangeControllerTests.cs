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

public class SpotCardExchangeControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds 3 catalog rows: a regular class-0 card, a regular class-1 card, and a pre-release
    /// card. Plus card-catalog rows so RewardGrantService can resolve the grant. Caller sets
    /// viewer SpotPoints.
    /// </summary>
    private static async Task SeedCatalog(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        if (!await db.Cards.AnyAsync(c => c.Id == 900100001L))
            db.Cards.Add(new ShadowverseCardEntry { Id = 900100001L, Name = "TestSpotNeutral", Rarity = Rarity.Bronze });
        if (!await db.Cards.AnyAsync(c => c.Id == 900100002L))
            db.Cards.Add(new ShadowverseCardEntry { Id = 900100002L, Name = "TestSpotClan1", Rarity = Rarity.Bronze });
        if (!await db.Cards.AnyAsync(c => c.Id == 900100099L))
            db.Cards.Add(new ShadowverseCardEntry { Id = 900100099L, Name = "TestSpotPreRelease", Rarity = Rarity.Gold });

        db.SpotCardExchangeCatalog.AddRange(
            new SpotCardExchangeEntry { Id = 900100001L, ClassId = 0, ExchangePoint = 3500, TsRotationId = 10001, IsPreRelease = false, IsEnabled = true },
            new SpotCardExchangeEntry { Id = 900100002L, ClassId = 1, ExchangePoint = 3500, TsRotationId = 10001, IsPreRelease = false, IsEnabled = true },
            new SpotCardExchangeEntry { Id = 900100099L, ClassId = 0, ExchangePoint = 1000, TsRotationId = 10001, IsPreRelease = true,  IsEnabled = true });
        await db.SaveChangesAsync();
    }

    private static async Task SetSpotPoints(SVSimTestFactory f, long viewerId, ulong points)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.SpotPoints = points;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Top_returns_9_clan_buckets_with_pre_relase_info_typo()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetSpotPoints(factory, viewerId, 5000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/spot_card_exchange/top",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("spot_point").GetInt32(), Is.EqualTo(5000));

        var ecl = root.GetProperty("exchangeable_card_list");
        Assert.That(ecl.GetArrayLength(), Is.EqualTo(9), "wire shape: array of exactly 9 clan buckets");

        // Clan 0 bucket should have 2 cards (class-0 neutral + pre-release in our seed).
        var clan0 = ecl[0].GetProperty("1");
        Assert.That(clan0.GetArrayLength(), Is.EqualTo(2));

        // Wire typo preserved
        Assert.That(root.TryGetProperty("pre_relase_info", out var prInfo), Is.True);
        Assert.That(root.TryGetProperty("pre_release_info", out _), Is.False, "the typo-free spelling must NOT be emitted");
        Assert.That(prInfo.GetProperty("pre_release_spot_card_exchange_limit").GetInt32(), Is.EqualTo(2));
        Assert.That(prInfo.GetProperty("is_pre_release").GetBoolean(), Is.True, "catalog has a pre-release card");
    }

    [Test]
    public async Task Exchange_debits_spot_points_and_grants_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetSpotPoints(factory, viewerId, 5000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":900100001,"exchange_point":3500}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(2));   // SpotCardPoint post-state + Card grant

        // Debit: SpotCardPoint type=12, id=0, post-state 1500 (5000 - 3500)
        var debit = rewardList[0];
        Assert.That(debit.GetProperty("reward_type").GetInt32(), Is.EqualTo(12));
        Assert.That(debit.GetProperty("reward_id").GetInt64(), Is.EqualTo(0));
        Assert.That(debit.GetProperty("reward_num").GetInt32(), Is.EqualTo(1500));

        // Grant: Card type=5, id=card id, count=1
        var grant = rewardList[1];
        Assert.That(grant.GetProperty("reward_type").GetInt32(), Is.EqualTo(5));
        Assert.That(grant.GetProperty("reward_id").GetInt64(), Is.EqualTo(900100001L));

        // ViewerSpotCardExchange + viewer.Cards persisted
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var record = await db.ViewerSpotCardExchanges.FirstOrDefaultAsync(e => e.ViewerId == viewerId && e.CardId == 900100001L);
        Assert.That(record, Is.Not.Null);
        var owned = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(owned.Cards.Any(c => c.Card.Id == 900100001L), Is.True);
        Assert.That(owned.Currency.SpotPoints, Is.EqualTo(1500UL));
    }

    [Test]
    public async Task Exchange_with_insufficient_points_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetSpotPoints(factory, viewerId, 100);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":900100001,"exchange_point":3500}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // No exchange row should have been created
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await db.ViewerSpotCardExchanges.CountAsync(e => e.ViewerId == viewerId), Is.EqualTo(0));
    }

    [Test]
    public async Task Exchange_already_exchanged_card_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetSpotPoints(factory, viewerId, 7000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var first = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":900100001,"exchange_point":3500}"""));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var second = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":900100001,"exchange_point":3500}"""));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Pre_release_limit_blocks_third_exchange_and_top_reports_LimitOver_status()
    {
        // Seed 3 pre-release cards; viewer can exchange 2 then hits the limit on the 3rd.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            for (int i = 0; i < 3; i++)
            {
                long cid = 800100001L + i;
                if (!await db.Cards.AnyAsync(c => c.Id == cid))
                    db.Cards.Add(new ShadowverseCardEntry { Id = cid, Name = $"PR{i}", Rarity = Rarity.Bronze });
                db.SpotCardExchangeCatalog.Add(new SpotCardExchangeEntry
                {
                    Id = cid, ClassId = 0, ExchangePoint = 100, TsRotationId = 10099, IsPreRelease = true, IsEnabled = true,
                });
            }
            await db.SaveChangesAsync();
        }
        await SetSpotPoints(factory, viewerId, 10000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // Two successful pre-release exchanges
        var r1 = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":800100001,"exchange_point":100}"""));
        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var r2 = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":800100002,"exchange_point":100}"""));
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Third one rejected by pre-release limit (limit==2)
        var r3 = await client.PostAsync("/spot_card_exchange/exchange",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","card_id":800100003,"exchange_point":100}"""));
        Assert.That(r3.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // /top should report status=2 (LimitOver) for the remaining pre-release card
        var top = await client.PostAsync("/spot_card_exchange/top",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var topBody = await top.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(topBody);
        var clan0 = doc.RootElement.GetProperty("exchangeable_card_list")[0].GetProperty("1");
        int? statusFor800100003 = null;
        foreach (var card in clan0.EnumerateArray())
        {
            if (card.GetProperty("card_id").GetInt64() == 800100003L)
                statusFor800100003 = card.GetProperty("exchange_status").GetInt32();
        }
        Assert.That(statusFor800100003, Is.EqualTo(2), "unexchanged pre-release card after hitting limit should show LimitOver");

        var prCount = doc.RootElement.GetProperty("pre_relase_info").GetProperty("pre_release_spot_card_exchange_count").GetInt32();
        Assert.That(prCount, Is.EqualTo(2));
    }
}
