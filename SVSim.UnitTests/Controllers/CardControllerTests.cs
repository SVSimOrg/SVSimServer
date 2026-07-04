using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// End-to-end coverage for POST /card/destruct. Exercises the controller's double-decode of
/// the JSON-string-in-JSON payload and the reward_list post-state-totals shape. The
/// validate/mutate logic itself is covered by <c>CardInventoryRepositoryTests</c>; the tests
/// here just confirm the wire contract.
/// </summary>
public class CardControllerTests
{
    private static StringContent DestructBody(string innerJson) =>
        new(
            $$"""{"card_id_number_array":{{JsonSerializer.Serialize(innerJson)}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""",
            Encoding.UTF8,
            "application/json");

    private static StringContent CreateBody(string innerJson) =>
        new(
            $$"""{"card_id_number_array":{{JsonSerializer.Serialize(innerJson)}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""",
            Encoding.UTF8,
            "application/json");

    [Test]
    public async Task Destruct_happy_path_returns_redether_and_card_post_totals()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 5, dustReward: 50);

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Inner JSON: cardId -> "<num>,<client_snapshot>". The snapshot is informational only.
        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"2,5\"}"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        // The ShadowverseTranslationMiddleware only fires for UnityPlayer UA requests; test
        // clients send plain HTTP so the controller's JSON is returned unwrapped.
        var rewardList = JsonDocument.Parse(body).RootElement
            .GetProperty("reward_list");

        // Two entries — one RedEther (type 1), one Card (type 5).
        var entries = rewardList.EnumerateArray()
            .Select(e => (Type: e.GetProperty("reward_type").GetInt32(),
                          Id:   e.GetProperty("reward_id").GetInt64(),
                          Num:  e.GetProperty("reward_num").GetInt32()))
            .ToList();

        Assert.That(entries, Has.Member((Type: 1, Id: 0L, Num: 100)),
            "RedEther post-state total = 2 * 50 = 100");
        Assert.That(entries, Has.Member((Type: 5, Id: 10001001L, Num: 3)),
            "Card post-state owned count = 5 - 2 = 3");
    }

    [Test]
    public async Task Destruct_without_auth_header_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();   // no auth header

        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"1,1\"}"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("",                      Description = "empty string")]
    [TestCase("not json",              Description = "non-JSON garbage")]
    [TestCase("{\"10001001\":\"1\"}",  Description = "value missing snapshot")]
    [TestCase("{\"10001001\":\"0,5\"}", Description = "num=0 not allowed")]
    [TestCase("{\"10001001\":\"-1,5\"}", Description = "negative num")]
    [TestCase("{\"abc\":\"1,5\"}",     Description = "non-numeric cardId")]
    [TestCase("{\"10001001\":5}",      Description = "value not a string")]
    [TestCase("[]",                    Description = "root must be object, not array")]
    public async Task Destruct_with_malformed_inner_json_returns_400(string innerJson)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct", DestructBody(innerJson));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("malformed_request"));
    }

    [Test]
    public async Task Destruct_with_empty_inner_object_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct", DestructBody("{}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("malformed_request"));
    }

    [Test]
    public async Task Destruct_unknown_card_returns_400_unknown_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"99999999\":\"1,0\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("unknown_card"));
    }

    [Test]
    public async Task Destruct_not_destructible_returns_400_not_destructible()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 0, craftCost: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"1,3\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("not_destructible"));
    }

    [Test]
    public async Task Destruct_protected_returns_400_card_protected()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50, isProtected: true);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"1,3\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("card_protected"));
    }

    [Test]
    public async Task Destruct_insufficient_cards_returns_400_insufficient_cards()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, dustReward: 50);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"3,2\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("insufficient_cards"));
    }

    [Test]
    public async Task Destruct_proceeds_when_client_possession_snapshot_disagrees_with_server()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Server has 3 owned; client thinks it has 5 (stale snapshot).
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Inner JSON: destruct 1, client snapshot=5 (disagrees with server count=3).
        // Spec: snapshot mismatch is warn-log only, never blocks the request.
        var response = await client.PostAsync("/card/destruct",
            DestructBody("{\"10001001\":\"1,5\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        var entries = JsonDocument.Parse(body).RootElement
            .GetProperty("reward_list")
            .EnumerateArray()
            .Select(e => (Type: e.GetProperty("reward_type").GetInt32(),
                          Id:   e.GetProperty("reward_id").GetInt64(),
                          Num:  e.GetProperty("reward_num").GetInt32()))
            .ToList();

        // Vials awarded based on actual server count, not client snapshot.
        Assert.That(entries, Has.Member((Type: 1, Id: 0L, Num: 50)));
        Assert.That(entries, Has.Member((Type: 5, Id: 10001001L, Num: 2)));
    }

    [Test]
    public async Task Create_happy_path_returns_redether_and_card_post_totals()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"2,0\"}"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        var entries = JsonDocument.Parse(body).RootElement
            .GetProperty("reward_list")
            .EnumerateArray()
            .Select(e => (Type: e.GetProperty("reward_type").GetInt32(),
                          Id:   e.GetProperty("reward_id").GetInt64(),
                          Num:  e.GetProperty("reward_num").GetInt32()))
            .ToList();

        // 1000 - (2 * 200) = 600
        Assert.That(entries, Has.Member((Type: 1, Id: 0L, Num: 600)),
            "RedEther post-state total = 1000 - 400 = 600");
        Assert.That(entries, Has.Member((Type: 5, Id: 10001001L, Num: 2)),
            "Card post-state owned count = 0 + 2 = 2");
    }

    [Test]
    public async Task Create_without_auth_header_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"1,0\"}"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("",                       Description = "empty string")]
    [TestCase("not json",               Description = "non-JSON garbage")]
    [TestCase("{\"10001001\":\"1\"}",   Description = "value missing snapshot")]
    [TestCase("{\"10001001\":\"0,0\"}", Description = "num=0 not allowed")]
    [TestCase("{\"10001001\":\"-1,0\"}", Description = "negative num")]
    [TestCase("{\"abc\":\"1,0\"}",      Description = "non-numeric cardId")]
    [TestCase("{\"10001001\":5}",       Description = "value not a string")]
    [TestCase("[]",                     Description = "root must be object, not array")]
    public async Task Create_with_malformed_inner_json_returns_400(string innerJson)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create", CreateBody(innerJson));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("malformed_request"));
    }

    [Test]
    public async Task Create_with_empty_inner_object_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create", CreateBody("{}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("malformed_request"));
    }

    [Test]
    public async Task Create_unknown_card_returns_400_unknown_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SetRedEtherAsync(viewerId, 1_000UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"99999999\":\"1,0\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("unknown_card"));
    }

    [Test]
    public async Task Create_not_craftable_returns_400_not_craftable()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 0, dustReward: 0);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"1,0\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("not_craftable"));
    }

    [Test]
    public async Task Create_would_exceed_max_copies_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"1,3\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("would_exceed_max_copies"));
    }

    [Test]
    public async Task Create_insufficient_vials_returns_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 100UL);   // half of needed
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"1,0\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("insufficient_vials"));
    }

    [Test]
    public async Task Create_proceeds_when_client_possession_snapshot_disagrees_with_server()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Server has 0 owned; client thinks it has 5 (stale snapshot).
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Inner JSON: create 1, client snapshot=5 (disagrees with server count=0).
        // Spec: snapshot mismatch is warn-log only, never blocks the request.
        var response = await client.PostAsync("/card/create",
            CreateBody("{\"10001001\":\"1,5\"}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        var entries = JsonDocument.Parse(body).RootElement
            .GetProperty("reward_list")
            .EnumerateArray()
            .Select(e => (Type: e.GetProperty("reward_type").GetInt32(),
                          Id:   e.GetProperty("reward_id").GetInt64(),
                          Num:  e.GetProperty("reward_num").GetInt32()))
            .ToList();

        // RedEther and card count based on actual server state, not client snapshot.
        Assert.That(entries, Has.Member((Type: 1, Id: 0L, Num: 800)),
            "RedEther post-state total = 1000 - 200 = 800");
        Assert.That(entries, Has.Member((Type: 5, Id: 10001001L, Num: 1)),
            "Card post-state owned count = 0 + 1 = 1");
    }

    private static StringContent ProtectBody(long cardId, bool isProtected) =>
        new(
            $$"""{"card_id":{{cardId}},"is_protected":{{(isProtected ? "true" : "false")}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""",
            Encoding.UTF8,
            "application/json");

    [Test]
    public async Task Protect_toggles_flag_for_owned_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/protect", ProtectBody(10001001L, isProtected: true));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        // Verify persisted flag
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).IsProtected, Is.True);
    }

    [Test]
    public async Task Protect_round_trip_unsets_flag()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, isProtected: true);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/protect", ProtectBody(10001001L, isProtected: false));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).IsProtected, Is.False);
    }

    [Test]
    public async Task Protect_without_auth_header_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/card/protect", ProtectBody(10001001L, isProtected: true));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Protect_unknown_card_returns_400_unknown_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/protect", ProtectBody(99_999_999L, isProtected: true));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
        Assert.That(body, Does.Contain("unknown_card"));
    }

    [Test]
    public async Task Protect_returns_empty_data_object()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 1);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/card/protect", ProtectBody(10001001L, isProtected: true));
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        // The translation middleware only wraps for UnityPlayer UA; test clients see the raw
        // controller payload, which for CardProtectResponse is an empty object.
        Assert.That(body.Trim(), Is.EqualTo("{}"));
    }

    [Test]
    public async Task Protect_then_load_index_emits_is_protected_one()
    {
        // Spec: /load/index user_card_list[].is_protected is an int wire value (0 or 1),
        // not a bool. Protect a card then verify /load/index round-trips the flag correctly.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Set the protect flag.
        var protectResponse = await client.PostAsync("/card/protect",
            ProtectBody(10001001L, isProtected: true));
        Assert.That(protectResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await protectResponse.Content.ReadAsStringAsync());

        // Call /load/index and parse user_card_list.
        const string IndexRequestJson =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"web","card_master_hash":""}""";
        var loadResponse = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, Encoding.UTF8, "application/json"));
        var loadBody = await loadResponse.Content.ReadAsStringAsync();
        Assert.That(loadResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), loadBody);

        var cardEntry = JsonDocument.Parse(loadBody).RootElement
            .GetProperty("user_card_list")
            .EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("card_id").GetInt64() == 10001001L);

        Assert.That(cardEntry.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined),
            "Expected card 10001001 in user_card_list");
        Assert.That(cardEntry.GetProperty("is_protected").GetInt32(), Is.EqualTo(1),
            "is_protected wire value must be 1 (int) after protect call");
    }
}
