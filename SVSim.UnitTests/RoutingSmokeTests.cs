using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.EmulatedEntrypoint;

namespace SVSim.UnitTests;

/// <summary>
/// Verifies the routing-prefix fix (audit step 5) actually exposes endpoints at the URLs the
/// client calls (no `api/` prefix). Posts plain JSON without UnityPlayer UA so the
/// translation middleware bypasses and we test routing in isolation.
/// </summary>
public class RoutingSmokeTests
{
    private sealed class TestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SVSimDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<SVSimDbContext>(opt => opt.UseInMemoryDatabase("RoutingSmoke"));
            });
        }
    }

    private const string ValidBaseRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    [Test]
    public async Task CheckSpecialTitle_resolves_to_CheckController()
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/check/special_title",
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 OK at /check/special_title, got {response.StatusCode}. " +
            "If 404, the routing prefix fix (audit step 5) didn't take.");

        var body = await response.Content.ReadAsStringAsync();
        // Plain-JSON path uses camelCase (System.Text.Json default); MessagePack [Key] only applies
        // to the Unity-UA encrypted path through ShadowverseTranslationMiddleware.
        Assert.That(body, Does.Contain("\"title_image_id\":\"0\""),
            "SpecialTitleCheck should return the built-in title id \"0\".");
    }

    [Test]
    public async Task ImmutableDataCardMaster_resolves_to_ImmutableDataController()
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/immutable_data/card_master",
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        // [Authorize] on SVSimController means an unauthenticated POST returns 401 — that still
        // proves routing resolved (404 would mean the path didn't bind).
        Assert.That((int)response.StatusCode, Is.Not.EqualTo((int)HttpStatusCode.NotFound),
            $"Expected /immutable_data/card_master to route; got 404. Body: " +
            await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task ImportViewer_route_resolves()
    {
        // /admin/import_viewer is AllowAnonymous so the route should at least be reachable
        // (probably returns 400 for missing steam_id with our empty BaseRequest body; we only
        // assert routing not deep behavior).
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/admin/import_viewer",
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound),
            "/admin/import_viewer didn't resolve to a controller — route registration broken.");
    }

    [Test]
    public async Task ApiPrefixedRoute_returns_404()
    {
        // The OLD broken path should now 404 — proves we dropped the `api/` prefix cleanly.
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/check/special_title",
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // Authenticated endpoints — we don't set up Steam auth in tests, so we just assert the
    // route resolves (anything other than 404). Auth-flow integration tests are a separate
    // problem — see PLAN.md status-log open item on body re-read.

    [TestCase("/practice/info")]
    [TestCase("/practice/deck_list")]
    [TestCase("/practice/start")]
    [TestCase("/practice/finish")]
    [TestCase("/deck/my_list")]
    [TestCase("/deck/info")]
    [TestCase("/deck/update")]
    [TestCase("/deck/update_name")]
    [TestCase("/deck/update_sleeve")]
    [TestCase("/deck/update_leader_skin")]
    [TestCase("/deck/update_random_leader_skin")]
    [TestCase("/deck/update_order")]
    [TestCase("/deck/delete_deck_list")]
    [TestCase("/deck/get_empty_deck_number")]
    [TestCase("/deck/set_deck_redis")]
    [TestCase("/arena_two_pick/top")]
    [TestCase("/arena_two_pick/entry")]
    [TestCase("/arena_two_pick/class_choose")]
    [TestCase("/arena_two_pick/card_choose")]
    [TestCase("/arena_two_pick/retire")]
    [TestCase("/arena_two_pick/finish")]
    [TestCase("/arena_two_pick_battle/do_matching")]
    [TestCase("/arena_two_pick_battle/finish")]
    [TestCase("/arena_colosseum/get_fee_info")]
    [TestCase("/arena/get_challenge_info")]
    [TestCase("/arena/get_challenge_ranking_history")]
    [TestCase("/check/check_time_slip_card_master_hash")]
    [TestCase("/rotation_rank_battle/do_matching")]
    [TestCase("/unlimited_rank_battle/do_matching")]
    [TestCase("/ai_rotation_rank_battle/start")]
    [TestCase("/ai_unlimited_rank_battle/start")]
    [TestCase("/rotation_rank_battle/finish")]
    [TestCase("/unlimited_rank_battle/finish")]
    [TestCase("/ai_rotation_rank_battle/finish")]
    [TestCase("/ai_unlimited_rank_battle/finish")]
    [TestCase("/rank_battle/force_finish")]
    [TestCase("/rank_battle/add_client_log")]
    [TestCase("/rank_battle/add_all_client_log")]
    [TestCase("/rank_battle/add_last_turn_log")]
    [TestCase("/rank_battle/get_latest_master_point")]
    [TestCase("/rotation_free_battle/do_matching")]
    [TestCase("/unlimited_free_battle/do_matching")]
    [TestCase("/rotation_free_battle/finish")]
    [TestCase("/unlimited_free_battle/finish")]
    [TestCase("/free_battle/force_finish")]
    // Guild endpoints (Task 5)
    [TestCase("/guild/info")]
    [TestCase("/guild/create")]
    [TestCase("/guild/breakup")]
    [TestCase("/guild/update")]
    [TestCase("/guild/update_description")]
    [TestCase("/guild/update_emblem")]
    [TestCase("/guild/search_guild")]
    [TestCase("/guild/emblem_list")]
    [TestCase("/guild/others_info")]
    [TestCase("/guild/friend_list")]
    [TestCase("/guild/invite_user_list")]
    [TestCase("/guild/invited_guild_list")]
    [TestCase("/guild/invite")]
    [TestCase("/guild/cancel_invite")]
    [TestCase("/guild/reject_invite")]
    [TestCase("/guild/join")]
    [TestCase("/guild/cancel_join_request")]
    [TestCase("/guild/join_request_list")]
    [TestCase("/guild/join_request_accept")]
    [TestCase("/guild/reject_join_request")]
    [TestCase("/guild/leave")]
    [TestCase("/guild/remove")]
    [TestCase("/guild/change_role")]
    // GuildChat endpoints (Task 5)
    [TestCase("/guild_chat/messages")]
    [TestCase("/guild_chat/post")]
    [TestCase("/guild_chat/add_deck")]
    [TestCase("/guild_chat/delete_deck")]
    [TestCase("/guild_chat/add_replay")]
    [TestCase("/guild_chat/replay_detail")]
    [TestCase("/guild_chat/deck_log")]
    public async Task Authenticated_route_resolves(string path)
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(path,
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound),
            $"{path} didn't resolve to a controller — route registration broken.");
    }
}
