using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class GiftControllerTests
{
    private const string BaseAuthBlock =
        @"""viewer_id"":""0"",""steam_id"":0,""steam_session_ticket"":""""";

    [Test]
    public async Task GiftTop_returns_five_tutorial_gifts_for_unclaimed_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/tutorial/gift_top",
            new StringContent($$"""{"page":1,{{BaseAuthBlock}}}""", Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        var presents = root.GetProperty("present_list");
        Assert.That(presents.GetArrayLength(), Is.EqualTo(5));

        // Expect the legendary pack entry (present_id 71478630) to be present.
        bool foundLegendaryGift = false;
        foreach (var p in presents.EnumerateArray())
        {
            if (p.GetProperty("present_id").GetString() == "71478630")
            {
                foundLegendaryGift = true;
                Assert.That(p.GetProperty("reward_type").GetString(), Is.EqualTo("4"));
                Assert.That(p.GetProperty("reward_detail_id").GetString(), Is.EqualTo("90001"));
                Assert.That(p.GetProperty("reward_count").GetString(), Is.EqualTo("1"));
                Assert.That(p.GetProperty("item_type").GetInt32(), Is.EqualTo(2));
                Assert.That(p.GetProperty("message").GetString(), Is.EqualTo("For completing the tutorial"));
            }
        }
        Assert.That(foundLegendaryGift, Is.True, "Legendary starter pack gift (71478630) must be in present_list.");

        Assert.That(root.GetProperty("present_history_list").GetArrayLength(), Is.EqualTo(0));
        Assert.That(root.GetProperty("limit_over_present_list").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task GiftReceive_grants_currency_and_items_then_history_is_populated()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var pre = await factory.GetViewerCurrencyAsync(viewerId);

        var requestJson = $$"""
        {"present_id_array":["71478626","71478627","71478628","71478629","71478630"],"state":1,{{BaseAuthBlock}}}
        """;
        var response = await client.PostAsync("/tutorial/gift_receive",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        // Five received ids echoed.
        var ids = root.GetProperty("received_ids").EnumerateArray()
            .Select(e => e.GetString()).ToHashSet();
        Assert.That(ids, Is.EquivalentTo(new[] { "71478626", "71478627", "71478628", "71478629", "71478630" }));

        // present_list emptied, history populated.
        Assert.That(root.GetProperty("present_list").GetArrayLength(), Is.EqualTo(0));
        Assert.That(root.GetProperty("present_history_list").GetArrayLength(), Is.EqualTo(5));

        // Currency credited: +400 red ether, +100 rupees.
        // Tutorial gift 71478626 has reward_type=1 — that's RedEther per UserGoods.Type, not Crystal.
        var post = await factory.GetViewerCurrencyAsync(viewerId);
        Assert.That(post.RedEther - pre.RedEther, Is.EqualTo(400UL));
        Assert.That(post.Rupees - pre.Rupees, Is.EqualTo(100UL));

        // reward_list carries post-state TOTALS, not deltas, per project_wire_reward_list_post_state.
        // After claiming gifts, the crystal/rupy entries in reward_list should equal viewer's post-grant totals.
        var rewardList = root.GetProperty("reward_list").EnumerateArray().ToList();
        var redEtherEntry = rewardList.First(e => e.GetProperty("reward_type").GetString() == "1");
        var rupyEntry     = rewardList.First(e => e.GetProperty("reward_type").GetString() == "9");
        Assert.That(redEtherEntry.GetProperty("reward_num").GetString(),
            Is.EqualTo(post.RedEther.ToString()),
            "reward_list currency entries must carry POST-STATE TOTALS, not gift deltas (client does direct assignment).");
        Assert.That(rupyEntry.GetProperty("reward_num").GetString(),
            Is.EqualTo(post.Rupees.ToString()));
    }

    [Test]
    public async Task GiftReceive_advances_tutorial_state_from_31_to_41()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"present_id_array":["71478626"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/tutorial/gift_receive",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        // Response carries the new step inline.
        Assert.That(root.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(41));
        // Only 1 of 5 gifts claimed → 4 remain unclaimed → badge state must be "still has presents".
        Assert.That(root.GetProperty("is_unreceived_present").GetBoolean(), Is.True,
            "Partial claim leaves 4 gifts unclaimed in present_list — is_unreceived_present " +
            "must reflect that so the client's inbox badge keeps surfacing.");
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(1));
        Assert.That(root.GetProperty("present_list").GetArrayLength(), Is.EqualTo(4));

        // Side effect: viewer state advanced to 41.
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(41));
    }

    [Test]
    public async Task GiftReceive_returns_empty_received_ids_on_idempotent_replay()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"present_id_array":["71478626","71478627"],"state":1,{{BaseAuthBlock}}}""";

        // First call grants both gifts.
        await client.PostAsync("/tutorial/gift_receive",
            new StringContent(json, Encoding.UTF8, "application/json"));

        // Second call (replay) must return empty received_ids / total_receive_count_list /
        // reward_list — these lists describe what THIS call granted, not what the client
        // asked for. Echoing requested ids would re-fire the client's "received N gifts"
        // popup and direct-assign the same post-state totals again.
        var second = await client.PostAsync("/tutorial/gift_receive",
            new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.That(root.GetProperty("received_ids").GetArrayLength(), Is.EqualTo(0),
            "Idempotent re-claim grants nothing → received_ids empty.");
        Assert.That(root.GetProperty("total_receive_count_list").GetArrayLength(), Is.EqualTo(0));
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));

        // present_history_list still includes the originally-claimed gifts.
        Assert.That(root.GetProperty("present_history_list").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task GiftReceive_echoes_persisted_tutorial_step_not_hardcoded_41()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        // Viewer is past the tutorial entirely (state=100). The gift_receive endpoint is
        // still reachable via /tutorial/gift_receive — a stale client retry, for instance.
        // The persistence side max-preserves (keeps state at 100); the response must echo
        // 100, not the hardcoded 41 the endpoint used to emit, or the client's tutorial
        // state machine regresses on a no-op retry.
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"present_id_array":["71478626"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/tutorial/gift_receive",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(100));
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(100));
    }

    [Test]
    public async Task GiftReceive_with_pre_owned_item_increments_existing_row()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);

        // Seed item 1 (= the 3-item gift's reward_detail_id) with count=5 pre-existing.
        // Any non-tutorial source could leave a viewer here — battlepass, future reward,
        // admin import. Gift 71478628 grants +3 of item 1; the existing row must be
        // found and incremented, not duplicated. The (ViewerId, ItemId) unique index
        // added 2026-05-25 would otherwise throw on SaveChanges → 500 to the client.
        await factory.SeedOwnedItemAsync(viewerId, itemId: 1, count: 5, itemName: "PreOwnedItem");
        await factory.SeedTutorialPresentsAsync(viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"present_id_array":["71478628"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/tutorial/gift_receive",
            new StringContent(json, Encoding.UTF8, "application/json"));

        var bodyStr = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), bodyStr);

        // Existing row was incremented to 8 (5 + 3), not duplicated.
        Assert.That(await factory.GetOwnedItemCountAsync(viewerId, 1), Is.EqualTo(8),
            "Pre-existing OwnedItemEntry must be found via the ThenIncluded Item nav; " +
            "otherwise RewardGrantService falls through to add a new row and the " +
            "(ViewerId, ItemId) unique index throws on SaveChanges.");

        // reward_list reflects the post-state total (8), not the gift delta (3).
        using var doc = JsonDocument.Parse(bodyStr);
        var itemEntry = doc.RootElement.GetProperty("reward_list").EnumerateArray()
            .First(e => e.GetProperty("reward_type").GetString() == "4"
                     && e.GetProperty("reward_id").GetString() == "1");
        Assert.That(itemEntry.GetProperty("reward_num").GetString(), Is.EqualTo("8"),
            "RewardNum carries the POST-STATE TOTAL — client direct-assigns it onto the " +
            "cached count, so emitting the delta would clobber on-screen inventory.");
    }

    [Test]
    public async Task GiftReceive_second_call_with_same_ids_does_not_double_grant()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var preFirst = await factory.GetViewerCurrencyAsync(viewerId);
        var json = $$"""{"present_id_array":["71478626","71478627"],"state":1,{{BaseAuthBlock}}}""";

        await client.PostAsync("/tutorial/gift_receive", new StringContent(json, Encoding.UTF8, "application/json"));
        var midPost = await factory.GetViewerCurrencyAsync(viewerId);
        Assert.That(midPost.RedEther - preFirst.RedEther, Is.EqualTo(400UL));
        Assert.That(midPost.Rupees - preFirst.Rupees, Is.EqualTo(100UL));

        var second = await client.PostAsync("/tutorial/gift_receive", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var finalPost = await factory.GetViewerCurrencyAsync(viewerId);
        Assert.That(finalPost.RedEther, Is.EqualTo(midPost.RedEther), "Second claim of same present_ids must not re-grant.");
        Assert.That(finalPost.Rupees, Is.EqualTo(midPost.Rupees));
    }

    [Test]
    public async Task GiftTop_prod_route_returns_same_content_as_tutorial_route()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var tutorial = await client.PostAsync("/tutorial/gift_top",
            new StringContent($$"""{"page":1,{{BaseAuthBlock}}}""", Encoding.UTF8, "application/json"));
        var prod = await client.PostAsync("/gift/top",
            new StringContent($$"""{"page":1,{{BaseAuthBlock}}}""", Encoding.UTF8, "application/json"));

        Assert.That(tutorial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(prod.StatusCode,     Is.EqualTo(HttpStatusCode.OK));

        using var tDoc = JsonDocument.Parse(await tutorial.Content.ReadAsStringAsync());
        using var pDoc = JsonDocument.Parse(await prod.Content.ReadAsStringAsync());

        // The two routes are pure aliases — present_list under both contains the same five
        // PresentIds (compare as a set; CreatedAt-tied ordering may differ).
        var tIds = tDoc.RootElement.GetProperty("present_list").EnumerateArray()
            .Select(p => p.GetProperty("present_id").GetString()).ToHashSet();
        var pIds = pDoc.RootElement.GetProperty("present_list").EnumerateArray()
            .Select(p => p.GetProperty("present_id").GetString()).ToHashSet();
        Assert.That(pIds, Is.EquivalentTo(tIds));
        Assert.That(pIds.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task GiftReceive_prod_route_does_not_advance_tutorial_state()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"present_id_array":["71478626"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/gift/receive_gift",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Prod route NEVER advances tutorial. tutorial_step echoes persisted state (still 31).
        Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(31));
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(31));
    }

    [Test]
    public async Task GiftReceive_state_3_deletes_without_grant_and_without_history()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 31);
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var preCurrency = await factory.GetViewerCurrencyAsync(viewerId);

        // Delete the red-ether gift (71478626 grants +400 RedEther on state=1).
        var json = $$"""{"present_id_array":["71478626"],"state":3,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/gift/receive_gift",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        // No currency granted.
        var postCurrency = await factory.GetViewerCurrencyAsync(viewerId);
        Assert.That(postCurrency.RedEther, Is.EqualTo(preCurrency.RedEther),
            "state=3 (MAIL_DELETE) must not grant.");

        // No reward_list / total_receive_count_list entries.
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));
        Assert.That(root.GetProperty("total_receive_count_list").GetArrayLength(), Is.EqualTo(0));

        // received_ids still reflects what was processed (the row transitioned to Deleted),
        // so the client knows the gift is gone from its inbox.
        Assert.That(root.GetProperty("received_ids").GetArrayLength(), Is.EqualTo(1));

        // The deleted gift does NOT appear in present_history_list — it's tombstoned, not archived.
        Assert.That(root.GetProperty("present_history_list").GetArrayLength(), Is.EqualTo(0));
        // ... and present_list now has 4 remaining unclaimed gifts.
        Assert.That(root.GetProperty("present_list").GetArrayLength(), Is.EqualTo(4));
    }

    [Test]
    public async Task Signup_creates_viewer_with_five_unclaimed_tutorial_presents()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        // Drive the real /tool/signup path via RegisterAnonymousViewer. SeedTutorialPresentsAsync
        // is NOT called here — the point of this test is that the production signup flow seeds
        // the rows on its own.
        var freshUdid = Guid.NewGuid();
        long viewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var v = await repo.RegisterAnonymousViewer(freshUdid);
            viewerId = v.Id;
        }

        // Verify five ViewerPresent rows exist for this viewer, all Unclaimed, all
        // Source="tutorial", with the expected PresentIds.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var rows = await db.ViewerPresents
                .Where(p => p.ViewerId == viewerId)
                .ToListAsync();
            Assert.That(rows.Count, Is.EqualTo(5),
                "RegisterAnonymousViewer must seed exactly the five TutorialPresents rows.");
            Assert.That(rows.All(r => r.Status == PresentStatus.Unclaimed), Is.True);
            Assert.That(rows.All(r => r.Source == "tutorial"), Is.True);

            var ids = rows.Select(r => r.PresentId).ToHashSet();
            Assert.That(ids, Is.EquivalentTo(new[]
                { "71478626", "71478627", "71478628", "71478629", "71478630" }));
        }
    }

    [Test]
    public async Task GiftReceive_with_card_reward_grants_card_via_inventory_service()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);
        long cardId = await factory.SeedCardAsync();

        // Seed a single non-tutorial Card present.
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerPresents.Add(new ViewerPresent
            {
                ViewerId = viewerId,
                PresentId = "card-gift-001",
                Status = PresentStatus.Unclaimed,
                RewardType = 5,                // UserGoodsType.Card
                RewardDetailId = cardId,
                RewardCount = 1,
                Message = "Test card grant",
                CreatedAt = DateTime.UtcNow,
                Source = "test",
            });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = $$"""{"present_id_array":["card-gift-001"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/gift/receive_gift",
            new StringContent(json, Encoding.UTF8, "application/json"));
        var bodyStr = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), bodyStr);

        // Verify the card landed in the viewer's collection.
        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var owned = await ctx2.Viewers.AsNoTracking()
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(owned.Cards.Any(c => c.Card.Id == cardId && c.Count > 0), Is.True,
            "Card reward_type=5 must round-trip through the gift mapper and InventoryService.GrantAsync.");
    }

    [Test]
    public async Task GiftReceive_with_sleeve_reward_grants_sleeve_via_inventory_service()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);

        const int sleeveId = 700100;
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.Sleeves.Add(new SleeveEntry { Id = sleeveId });
            await ctx.SaveChangesAsync();

            ctx.ViewerPresents.Add(new ViewerPresent
            {
                ViewerId = viewerId,
                PresentId = "sleeve-gift-001",
                Status = PresentStatus.Unclaimed,
                RewardType = 6,                // UserGoodsType.Sleeve
                RewardDetailId = sleeveId,
                RewardCount = 1,
                Message = "Test sleeve grant",
                CreatedAt = DateTime.UtcNow,
                Source = "test",
            });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = $$"""{"present_id_array":["sleeve-gift-001"],"state":1,{{BaseAuthBlock}}}""";
        var response = await client.PostAsync("/gift/receive_gift",
            new StringContent(json, Encoding.UTF8, "application/json"));
        var bodyStr = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), bodyStr);

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var owned = await ctx2.Viewers.AsNoTracking()
            .Include(v => v.Sleeves)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(owned.Sleeves.Any(s => s.Id == sleeveId), Is.True,
            "Sleeve reward_type=6 must round-trip through the gift mapper and InventoryService.GrantAsync.");
    }

    [Test]
    public async Task GiftReceive_with_unsupported_reward_type_does_not_grant()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);

        // RewardType 11 = SpotCard, which GiftRewardTypes.IsSupported rejects, causing
        // WireRewardTypeToUserGoodsType to throw InvalidOperationException before CommitAsync.
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerPresents.Add(new ViewerPresent
            {
                ViewerId = viewerId,
                PresentId = "bad-gift-001",
                Status = PresentStatus.Unclaimed,
                RewardType = 11,
                RewardDetailId = 123,
                RewardCount = 1,
                Message = "Bad type",
                CreatedAt = DateTime.UtcNow,
                Source = "test",
            });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = $$"""{"present_id_array":["bad-gift-001"],"state":1,{{BaseAuthBlock}}}""";

        // TestServer propagates unhandled controller exceptions through HttpClient.PostAsync
        // (because Program.cs has no app.UseExceptionHandler middleware). Catch the exception
        // here; the critical assertion is that the DB row was never transitioned.
        bool threw = false;
        try
        {
            var response = await client.PostAsync("/gift/receive_gift",
                new StringContent(json, Encoding.UTF8, "application/json"));
            // If exception handling middleware is added later, the response will be 500 — accept both.
            Assert.That((int)response.StatusCode, Is.GreaterThanOrEqualTo(500),
                "Unsupported reward_type must not return a success status code.");
        }
        catch (Exception ex) when (ex.Message.Contains("Unsupported gift reward_type") ||
                                   ex.InnerException?.Message.Contains("Unsupported gift reward_type") == true)
        {
            threw = true;
        }

        // Either path (thrown exception or 5xx) is acceptable; what matters is the DB row stayed Unclaimed.
        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var present = await ctx2.ViewerPresents.AsNoTracking()
            .FirstAsync(p => p.PresentId == "bad-gift-001");
        Assert.That(present.Status, Is.EqualTo(PresentStatus.Unclaimed),
            "Failed claim must NOT transition the row — it's still claimable once the producer is fixed.");

        // Confirm the exception did propagate (not swallowed into a silent 200).
        Assert.That(threw, Is.True,
            "InvalidOperationException for unsupported reward_type must propagate — not be silently swallowed into a 200.");
    }
}
