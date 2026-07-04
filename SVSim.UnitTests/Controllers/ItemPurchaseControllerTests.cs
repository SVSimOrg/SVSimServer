using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ItemPurchaseControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Seeds two catalog entries:
    ///   #501: lifetime quota 1, costs 100 RedEther → 1 Item(1000)
    ///   #502: monthly quota 3,  costs 5 Item(1001) → 1 Item(1000)
    /// Plus the Item rows (1000, 1001) needed by RewardGrantService.
    /// Caller seeds the viewer with starting currency/items.
    /// </summary>
    private static async Task SeedCatalog(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        if (!await db.Items.AnyAsync(i => i.Id == 1000))
            db.Items.Add(new ItemEntry { Id = 1000, Name = "Seer's Globe", Type = 3, ThumbnailPath = "thumbnail_orb" });
        if (!await db.Items.AnyAsync(i => i.Id == 1001))
            db.Items.Add(new ItemEntry { Id = 1001, Name = "Seer's Globe Shards", Type = 5, ThumbnailPath = "thumbnail_orb_piece" });

        db.ItemPurchaseCatalog.AddRange(
            new ItemPurchaseCatalogEntry
            {
                Id = 501, IsEnabled = true,
                RequireItemType = 1, RequireItemId = 0, RequireItemNum = 100,   // 100 RedEther
                PurchaseItemType = 4, PurchaseItemId = 1000, PurchaseItemNum = 1, // → 1 Globe
                PurchaseName = "Lifetime Globe", IsMonthlyReset = false, PurchaseLimit = 1,
            },
            new ItemPurchaseCatalogEntry
            {
                Id = 502, IsEnabled = true,
                RequireItemType = 4, RequireItemId = 1001, RequireItemNum = 5,    // 5 Shards
                PurchaseItemType = 4, PurchaseItemId = 1000, PurchaseItemNum = 1, // → 1 Globe
                PurchaseName = "Monthly Globe", IsMonthlyReset = true, PurchaseLimit = 3,
            });
        await db.SaveChangesAsync();
    }

    private static async Task SetViewerCurrency(SVSimTestFactory f, long viewerId, ulong redEther = 0)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.RedEther = redEther;
        await db.SaveChangesAsync();
    }

    private static async Task SetViewerItem(SVSimTestFactory f, long viewerId, int itemId, int count)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var item = await db.Items.FindAsync(itemId);
        var v = await db.Viewers.Include(x => x.Items).ThenInclude(i => i.Item).FirstAsync(x => x.Id == viewerId);
        var owned = v.Items.FirstOrDefault(i => i.Item.Id == itemId);
        if (owned is null)
            v.Items.Add(new OwnedItemEntry { Item = item!, Count = count, Viewer = v });
        else
            owned.Count = count;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Info_returns_catalog_and_full_ticket_list()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();  // loads item catalog including Type==2 tickets
        await SeedCatalog(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_purchase/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var info = doc.RootElement.GetProperty("item_purchase_info");
        Assert.That(info.GetArrayLength(), Is.GreaterThanOrEqualTo(2), "should include seeded entries 501 and 502");

        var entry501 = FindEntry(info, 501);
        Assert.That(entry501.GetProperty("require_item_num").GetInt32(), Is.EqualTo(100));
        Assert.That(entry501.GetProperty("is_monthly_reset").GetInt32(), Is.EqualTo(0));
        Assert.That(entry501.GetProperty("rest").GetInt32(), Is.EqualTo(1));

        var entry502 = FindEntry(info, 502);
        Assert.That(entry502.GetProperty("is_monthly_reset").GetInt32(), Is.EqualTo(1));
        Assert.That(entry502.GetProperty("rest").GetInt32(), Is.EqualTo(3));

        // Ticket list should include every Type==2 item — seeded items.json has ~33 such rows.
        var tickets = doc.RootElement.GetProperty("user_card_pack_ticket_list");
        Assert.That(tickets.GetArrayLength(), Is.GreaterThan(10), "all Type==2 items should be listed");
        // First-element shape check
        Assert.That(tickets[0].GetProperty("item_id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Purchase_with_red_ether_debits_and_grants_item()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetViewerCurrency(factory, viewerId, redEther: 5000);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":501,"rest":1}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.EqualTo(2));   // debit post-state + grant

        // Debit: RedEther type=1, id=0, post-state total 4900
        var debit = rewardList[0];
        Assert.That(debit.GetProperty("reward_type").GetInt32(), Is.EqualTo(1));
        Assert.That(debit.GetProperty("reward_id").GetInt64(), Is.EqualTo(0));
        Assert.That(debit.GetProperty("reward_num").GetInt32(), Is.EqualTo(4900));

        // Grant: Item type=4, id=1000, count=1 (viewer didn't have any before)
        var grant = rewardList[1];
        Assert.That(grant.GetProperty("reward_type").GetInt32(), Is.EqualTo(4));
        Assert.That(grant.GetProperty("reward_id").GetInt64(), Is.EqualTo(1000));
        Assert.That(grant.GetProperty("reward_num").GetInt32(), Is.EqualTo(1));

        // Counter row should exist for lifetime quota
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var counter = await db.ViewerEventCounters
            .FirstOrDefaultAsync(c => c.ViewerId == viewerId && c.EventKey == "item_purchase:501");
        Assert.That(counter, Is.Not.Null);
        Assert.That(counter!.Period, Is.EqualTo("all-time"));
        Assert.That(counter.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Purchase_with_item_currency_debits_and_grants_item()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetViewerItem(factory, viewerId, itemId: 1001, count: 12);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":502,"rest":3}"""));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        // Debit Item(1001) 12 → 7, grant Item(1000) 0 → 1
        var debit = rewardList[0];
        Assert.That(debit.GetProperty("reward_type").GetInt32(), Is.EqualTo(4));
        Assert.That(debit.GetProperty("reward_id").GetInt64(), Is.EqualTo(1001));
        Assert.That(debit.GetProperty("reward_num").GetInt32(), Is.EqualTo(7));
    }

    [Test]
    public async Task Purchase_sold_out_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetViewerCurrency(factory, viewerId, redEther: 500);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // First buy succeeds (entry 501 is lifetime quota 1)
        var first = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":501,"rest":1}"""));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Second buy rejected as sold_out — currency check is never reached
        var second = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":501,"rest":0}"""));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Purchase_with_insufficient_red_ether_returns_400_and_does_not_increment_counter()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetViewerCurrency(factory, viewerId, redEther: 50);   // < 100 required

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":501,"rest":1}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Counter must NOT have been incremented — quota stays at 1.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var counter = await db.ViewerEventCounters
            .FirstOrDefaultAsync(c => c.ViewerId == viewerId && c.EventKey == "item_purchase:501");
        Assert.That(counter, Is.Null);
    }

    [Test]
    public async Task Purchase_unknown_purchase_id_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // No SeedCatalog — purchase_id 501 doesn't exist
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_purchase/purchase",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":501,"rest":1}"""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Monthly_quota_decrements_rest_on_repeat_buys_within_same_period()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCatalog(factory);
        await SetViewerItem(factory, viewerId, itemId: 1001, count: 20);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // entry 502 is monthly quota 3; buy twice
        for (int i = 0; i < 2; i++)
        {
            var resp = await client.PostAsync("/item_purchase/purchase",
                JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","purchase_id":502,"rest":3}"""));
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // /info should now report rest=1
        var info = await client.PostAsync("/item_purchase/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var body = await info.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var entry502 = FindEntry(doc.RootElement.GetProperty("item_purchase_info"), 502);
        Assert.That(entry502.GetProperty("rest").GetInt32(), Is.EqualTo(1));
    }

    private static JsonElement FindEntry(JsonElement array, int purchaseId)
    {
        foreach (var entry in array.EnumerateArray())
        {
            if (entry.GetProperty("purchase_id").GetInt32() == purchaseId)
                return entry;
        }
        throw new InvalidOperationException($"entry with purchase_id={purchaseId} not found");
    }
}
