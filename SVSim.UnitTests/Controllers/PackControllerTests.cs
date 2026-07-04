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

public class PackControllerTests
{
    [Test]
    public async Task PackInfo_item_number_reflects_owned_ticket_count()
    {
        // Verifies the ownedItemsByItemId projection in PackController.Info — the dict that
        // drives child_gacha_info.item_number. Tutorial flow filters packs by item_number > 0,
        // so a regression on the projection (e.g. nav-eval collapsing to 0) silently hides
        // any pack that requires a ticket.
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 41);

        // Seed item 90001 with count 7 — the legendary starter ticket the tutorial gift grants.
        await factory.SeedOwnedItemAsync(viewerId, itemId: 90001, count: 7, itemName: "Starter Legendary Ticket");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/pack/info",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Find pack 99047 (the starter legendary) and verify its child gacha reports item_number=7.
        var pack99047 = doc.RootElement.GetProperty("pack_config_list").EnumerateArray()
            .First(p => p.GetProperty("parent_gacha_id").GetInt32() == 99047);
        var childWithTicket = pack99047.GetProperty("child_gacha_info").EnumerateArray()
            .First(c => c.TryGetProperty("item_id", out var iid) && iid.GetString() == "90001");
        Assert.That(childWithTicket.GetProperty("item_number").GetInt32(), Is.EqualTo(7),
            "child_gacha_info.item_number must reflect the viewer's owned count of the gating " +
            "item; client filters tutorial packs on item_number > 0.");
    }

    [Test]
    public async Task TutorialPackInfo_returns_same_list_as_pack_info()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 41);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var direct = await client.PostAsync("/pack/info", new StringContent(json, Encoding.UTF8, "application/json"));
        var tutorial = await client.PostAsync("/tutorial/pack_info", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(direct.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(tutorial.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var directBody = await direct.Content.ReadAsStringAsync();
        var tutorialBody = await tutorial.Content.ReadAsStringAsync();
        Assert.That(tutorialBody, Is.EqualTo(directBody),
            "tutorial/pack_info wire shape must match /pack/info exactly (no filtering in v1).");
    }

    [Test]
    public async Task TutorialPackOpen_grants_pack_and_sets_tutorial_step_100()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 41);

        // Seed the starter ticket the gift_receive step would have granted. /tutorial/pack_open
        // is supposed to decrement this count by `pack_number` (1) and emit a post-state entry
        // into reward_list (per project_wire_reward_list_post_state).
        await factory.SeedOwnedItemAsync(viewerId, itemId: 90001, count: 1, itemName: "Starter Legendary Ticket");

        // Pack 99047 (starter legendary) has base_pack_id=90001. The minimal card seed only
        // creates set 10001, so we seed set 90001 explicitly for the pool resolver.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.CardSets.Add(new ShadowverseCardSetEntry
            {
                Id = 90001,
                Name = "TutorialStarterSet",
                IsInRotation = true,
                IsBasic = false,
                Cards =
                [
                    new ShadowverseCardEntry { Id = 90001001L, Name = "StarterCard1", Rarity = Rarity.Bronze },
                    new ShadowverseCardEntry { Id = 90001002L, Name = "StarterCard2", Rarity = Rarity.Gold },
                    new ShadowverseCardEntry { Id = 90001003L, Name = "StarterCard3", Rarity = Rarity.Legendary },
                ],
            });
            await db.SaveChangesAsync();
        }
        // Install a draw table for 99047 pointing at the seeded starter cards.
        await factory.SeedPackDrawTableAsync(99047, 90001001L, 90001002L, 90001003L);

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"parent_gacha_id":99047,"gacha_id":990047,"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var response = await client.PostAsync("/tutorial/pack_open",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(100),
            "tutorial/pack_open must include tutorial_step=100 in data — this is the END transition.");
        Assert.That(root.GetProperty("pack_list").GetArrayLength(), Is.EqualTo(8),
            "Starter pack 99047/990047 delivers 8 cards (child_gacha.card_count=8).");

        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(100));

        // Ticket decrement: the legendary starter ticket (90001) should be consumed.
        Assert.That(await factory.GetOwnedItemCountAsync(viewerId, 90001), Is.EqualTo(0),
            "Tutorial pack_open must decrement the gating ticket; otherwise /tutorial/pack_info " +
            "keeps showing the pack and the client re-clicks into /pack/open (501 on type_detail=5).");

        // reward_list must carry a post-state item entry for the ticket. RewardType=4 (Item),
        // RewardId=90001, RewardNum=0 (post-state total, NOT delta).
        var rewardList = root.GetProperty("reward_list");
        var ticketEntry = rewardList.EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("reward_type").GetInt32() == 4
                              && e.GetProperty("reward_id").GetInt64() == 90001);
        Assert.That(ticketEntry.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined),
            "reward_list must include a type=4 entry for the consumed ticket (90001) so the " +
            "client's _userItemDict updates immediately — project_wire_reward_list_post_state.");
        Assert.That(ticketEntry.GetProperty("reward_num").GetInt32(), Is.EqualTo(0),
            "RewardNum is the post-state TOTAL, not the delta consumed.");
    }

    [Test]
    public async Task NonTutorial_pack_open_does_not_emit_tutorial_step()
    {
        // Verify that regular /pack/open works on a ticket-funded pack AND does not include
        // tutorial_step in the response. Pack 99047 uses type_detail=5 (TICKET_MULTI), which
        // the non-tutorial path now accepts: a normal viewer with a ticket buys a normal pack
        // — only the /tutorial/pack_open alias attaches tutorial_step.
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 100);

        // Pack 99047 has BasePackId=90001 (Throwback). The minimal card seed only creates set
        // 10001, so seed set 90001 explicitly + install a draw table pointing at its cards.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.CardSets.Add(new ShadowverseCardSetEntry
            {
                Id = 90001, Name = "TutorialStarterSet", IsInRotation = true, IsBasic = false,
                Cards =
                [
                    new ShadowverseCardEntry { Id = 90001001L, Name = "StarterCard1", Rarity = Rarity.Bronze },
                    new ShadowverseCardEntry { Id = 90001002L, Name = "StarterCard2", Rarity = Rarity.Gold },
                    new ShadowverseCardEntry { Id = 90001003L, Name = "StarterCard3", Rarity = Rarity.Legendary },
                ],
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableAsync(99047, 90001001L, 90001002L, 90001003L);
        await factory.SeedOwnedItemAsync(viewerId, itemId: 90001, count: 1, itemName: "Starter Legendary Ticket");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"parent_gacha_id":99047,"gacha_id":990047,"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/pack/open",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(body.Contains("\"tutorial_step\""), Is.False,
            "Regular /pack/open must never emit tutorial_step — only /tutorial/pack_open does.");
    }

    [Test]
    public async Task TutorialPackOpen_does_not_downgrade_state_past_100()
    {
        // Max-preserve: a state > 100 (e.g., a post-tutorial training sentinel) must not be
        // clobbered down to 100. Nothing in prod sets state above 100 today, so synthesize
        // the case directly. The viewer must have the starter ticket + card pool seeded so
        // the path reaches the tutorial epilogue (rather than 400ing before the state write).
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 200);
        await factory.SeedOwnedItemAsync(viewerId, itemId: 90001, count: 1, itemName: "Starter Legendary Ticket");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.CardSets.Add(new ShadowverseCardSetEntry
            {
                Id = 90001, Name = "TutorialStarterSet", IsInRotation = true, IsBasic = false,
                Cards =
                [
                    new ShadowverseCardEntry { Id = 90001001L, Name = "StarterCard1", Rarity = Rarity.Bronze },
                    new ShadowverseCardEntry { Id = 90001002L, Name = "StarterCard2", Rarity = Rarity.Gold },
                    new ShadowverseCardEntry { Id = 90001003L, Name = "StarterCard3", Rarity = Rarity.Legendary },
                ],
            });
            await db.SaveChangesAsync();
        }
        await factory.SeedPackDrawTableAsync(99047, 90001001L, 90001002L, 90001003L);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = """{"parent_gacha_id":99047,"gacha_id":990047,"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/tutorial/pack_open",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.GreaterThanOrEqualTo(200),
            "TutorialState must not regress when the alias fires against a viewer past END.");
    }
}
