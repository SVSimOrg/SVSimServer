using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class TutorialFlowEndToEndTests
{
    [Test]
    public async Task FreshSignup_through_pack_open_reaches_tutorial_step_100()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();

        // Fresh viewer at PRE_TUTORIAL_STEP (the real prod default after Task 1).
        // SeedViewerAsync goes through RegisterViewer (admin/social path) which does NOT
        // auto-seed tutorial presents — only the prod /tool/signup -> RegisterAnonymousViewer
        // flow does. The end-to-end simulation here needs the inbox populated explicitly.
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        await factory.SeedTutorialPresentsAsync(viewerId);

        // Pack 99047 (starter legendary) has base_pack_id=90001. Seed the card set used by
        // the tutorial pack pool resolver — mirrors the pattern in PackControllerTests.
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

        var preCurrency = await factory.GetViewerCurrencyAsync(viewerId);

        // 1. /account/update_name (after name-entry screen).
        var nameResp = await Post(client, "/account/update_name",
            """{"name":"e2e_test_user","viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        Assert.That(nameResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 2. Step transitions observed in capture: 11 → 21 → 31.
        foreach (var step in new[] { 11, 21, 31 })
        {
            var json = $$"""{"tutorial_step":{{step}},"is_skip":0,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
            var resp = await Post(client, "/tutorial/update", json);
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(step));
        }

        // 3. /tutorial/update_action — a couple of representative sub-step calls.
        await Post(client, "/tutorial/update_action",
            """{"tutorial_step":1,"tutorial_action_number":2,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");

        // 4. /tutorial/gift_top — surface the bundle.
        var topResp = await Post(client, "/tutorial/gift_top",
            """{"page":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        using (var doc = JsonDocument.Parse(await topResp.Content.ReadAsStringAsync()))
        {
            Assert.That(doc.RootElement.GetProperty("present_list").GetArrayLength(), Is.EqualTo(5));
        }

        // 5. /tutorial/gift_receive — claim them.
        var receiveResp = await Post(client, "/tutorial/gift_receive",
            """{"present_id_array":["71478630","71478629","71478628","71478627","71478626"],"state":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        Assert.That(receiveResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var midCurrency = await factory.GetViewerCurrencyAsync(viewerId);
        // Tutorial gift 71478626 has reward_type=1 — that's RedEther per UserGoods.Type, not Crystal.
        Assert.That(midCurrency.RedEther - preCurrency.RedEther, Is.EqualTo(400UL));
        Assert.That(midCurrency.Rupees - preCurrency.Rupees, Is.EqualTo(100UL));

        // gift_receive should also have advanced the tutorial step to 41 server-side.
        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(41));

        // 6. /tutorial/pack_info — show the 3 active packs.
        var packInfoResp = await Post(client, "/tutorial/pack_info",
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        Assert.That(packInfoResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 7. /tutorial/pack_open of the starter legendary pack — END transition.
        var openBody = """{"parent_gacha_id":99047,"gacha_id":990047,"gacha_type":1,"pack_number":1,"exclude_card_ids":[],"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var openResp = await Post(client, "/tutorial/pack_open", openBody);
        var openRespBody = await openResp.Content.ReadAsStringAsync();
        Assert.That(openResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), openRespBody);

        using (var doc = JsonDocument.Parse(openRespBody))
        {
            Assert.That(doc.RootElement.GetProperty("tutorial_step").GetInt32(), Is.EqualTo(100));
        }

        Assert.That(await factory.GetViewerTutorialStateAsync(viewerId), Is.EqualTo(100),
            "Viewer reaches TUTORIAL_END after the full flow.");

        // The gift granted item 90001 count=1 (via /tutorial/gift_receive entry 71478630).
        // /tutorial/pack_open consumes it; assert the ticket is gone post-flow.
        Assert.That(await factory.GetOwnedItemCountAsync(viewerId, 90001), Is.EqualTo(0),
            "Starter legendary ticket must be consumed by /tutorial/pack_open.");
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string url, string body)
        => client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
}
