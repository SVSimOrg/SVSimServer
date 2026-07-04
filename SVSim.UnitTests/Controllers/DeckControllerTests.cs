using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>/deck/*</c> — the deck-editor CRUD surface. Tests assert against the
/// <c>[Key("...")]</c>-driven wire keys (mirrored to <c>[JsonPropertyName]</c>); these are
/// the names the decompiled client actually parses, NOT <c>SnakeCaseLower(C# property)</c>.
/// </summary>
public class DeckControllerTests
{
    // ToApi() converts internal Format -> wire deck_format int (e.g. Format.Rotation -> 1).
    // Tests MUST send wire values; the controller routes them back via FormatExtensions.FromApi.
    // Inline `"deck_format":1` literals below correspond to Format.Rotation (the format the
    // SeedDeckAsync fixtures use).
    private static string DeckFormatRequestJson(Format f) =>
        $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_format":{{f.ToApi()}}}""";

    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<(int classId, int sleeveId, int leaderSkinId)> FetchSeededIds(SVSimTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var cls = await db.Classes.Select(c => c.Id).FirstAsync();
        var sleeve = await db.Sleeves.Select(s => s.Id).FirstAsync();
        var skin = await db.LeaderSkins.Select(s => s.Id).FirstAsync();
        return (cls, sleeve, skin);
    }

    // ---- read endpoints ----

    [Test]
    public async Task MyList_returns_decks_for_format()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, "Slot 1");
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 2, "Slot 2");
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, 1, "Wrong-format deck");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/deck/my_list", JsonBody(DeckFormatRequestJson(Format.Rotation)));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var decks = doc.RootElement.GetProperty("user_deck_list");
        // Real decks are tagged is_complete_deck=1; padding placeholders are 0.
        var realNames = Enumerable.Range(0, decks.GetArrayLength())
            .Select(i => decks[i])
            .Where(d => d.GetProperty("is_complete_deck").GetInt32() == 1)
            .Select(d => d.GetProperty("deck_name").GetString())
            .ToList();
        Assert.That(realNames, Is.EquivalentTo(new[] { "Slot 1", "Slot 2" }),
            "Only Rotation-format decks should be returned for a Rotation request.");
    }

    [Test]
    public async Task Info_returns_decks_for_format()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, 1, "Unlimited Deck");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/deck/info", JsonBody(DeckFormatRequestJson(Format.Unlimited)));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var decks = doc.RootElement.GetProperty("user_deck_list");
        var realDecks = Enumerable.Range(0, decks.GetArrayLength())
            .Select(i => decks[i])
            .Where(d => d.GetProperty("is_complete_deck").GetInt32() == 1)
            .ToList();
        Assert.That(realDecks.Count, Is.EqualTo(1));
        Assert.That(realDecks[0].GetProperty("deck_name").GetString(), Is.EqualTo("Unlimited Deck"));
    }

    [Test]
    public async Task MyList_empty_when_viewer_has_no_decks()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/deck/my_list", JsonBody(DeckFormatRequestJson(Format.Rotation)));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var decks = doc.RootElement.GetProperty("user_deck_list");
        var realDeckCount = Enumerable.Range(0, decks.GetArrayLength())
            .Count(i => decks[i].GetProperty("is_complete_deck").GetInt32() == 1);
        Assert.That(realDeckCount, Is.EqualTo(0));
    }

    [Test]
    public async Task MyList_pads_response_to_max_deck_slots_with_empty_placeholders()
    {
        // The client only renders a "New Deck" tile by converting the first response entry whose
        // card_id_array is [] into a CreateNew slot (DeckUI.DeckViewData.CreateDeckViewList).
        // Prod always pads the deck list to the per-format cap (36 in the 2026-05-23 capture)
        // with empty placeholders. Without padding, the button never appears.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, "Slot 1");
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 2, "Slot 2");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/deck/my_list", JsonBody(DeckFormatRequestJson(Format.Rotation)));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var decks = doc.RootElement.GetProperty("user_deck_list");
        Assert.That(decks.GetArrayLength(), Is.EqualTo(36),
            "Response should pad to MaxDeckSlots (36) so the client can render the New Deck tile.");

        // Real decks have is_complete_deck=1; placeholders have is_complete_deck=0. This is the
        // distinguishing marker the client itself uses (DeckData.is_complete_deck in DeckData.cs).
        var empties = Enumerable.Range(0, decks.GetArrayLength())
            .Select(i => decks[i])
            .Where(d => d.GetProperty("is_complete_deck").GetInt32() == 0)
            .ToList();
        Assert.That(empties.Count, Is.EqualTo(34),
            "Two real decks + 34 empty placeholders = 36 slots.");

        var firstEmpty = empties[0];
        Assert.That(firstEmpty.GetProperty("deck_name").GetString(), Is.EqualTo(""));
        Assert.That(firstEmpty.GetProperty("card_id_array").GetArrayLength(), Is.EqualTo(0));
        Assert.That(firstEmpty.GetProperty("class_id").GetInt32(), Is.EqualTo(1));
        Assert.That(firstEmpty.GetProperty("sleeve_id").GetInt32(), Is.EqualTo(3000011));
        Assert.That(firstEmpty.GetProperty("is_available_deck").GetInt32(), Is.EqualTo(1));
        // Padded slot numbers must not collide with real ones, and together they must cover [1..36].
        var allDeckNos = Enumerable.Range(0, decks.GetArrayLength())
            .Select(i => decks[i].GetProperty("deck_no").GetInt32())
            .OrderBy(n => n)
            .ToList();
        Assert.That(allDeckNos, Is.EqualTo(Enumerable.Range(1, 36).ToList()));
    }

    // ---- get_empty_deck_number ----

    [Test]
    public async Task GetEmptyDeckNumber_returns_1_when_viewer_has_no_decks()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/deck/get_empty_deck_number",
            JsonBody(DeckFormatRequestJson(Format.Rotation)));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("empty_deck_num").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetEmptyDeckNumber_returns_next_free_slot_when_slots_filled()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 2);
        // Skip slot 3 so the algorithm should hand it back.
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 4);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/deck/get_empty_deck_number",
            JsonBody(DeckFormatRequestJson(Format.Rotation)));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("empty_deck_num").GetInt32(), Is.EqualTo(3),
            "Algorithm must return the smallest free slot, not just one past the highest used.");
    }

    // ---- update (create / update / delete) ----

    [Test]
    public async Task Update_creates_new_deck_when_slot_empty()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var (classId, sleeveId, leaderSkinId) = await FetchSeededIds(factory);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var updateJson = $$"""
            {"viewer_id":"0","steam_id":0,"steam_session_ticket":"",
             "deck_no":1,"class_id":{{classId}},"leader_skin_id":{{leaderSkinId}},
             "is_random_leader_skin":false,"sleeve_id":{{sleeveId}},"deck_name":"Fresh Deck",
             "is_delete":0,"deck_format":1}
            """;
        var response = await client.PostAsync("/deck/update", JsonBody(updateJson));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var count = await db.Decks.CountAsync(d => d.Number == 1 && d.Format == Format.Rotation);
        Assert.That(count, Is.EqualTo(1), "A new deck row should have been inserted.");
        var persisted = await db.Decks.FirstAsync(d => d.Number == 1 && d.Format == Format.Rotation);
        Assert.That(persisted.Name, Is.EqualTo("Fresh Deck"));
    }

    [Test]
    public async Task Update_updates_existing_deck_in_place()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, name: "Original");
        var (classId, sleeveId, leaderSkinId) = await FetchSeededIds(factory);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var updateJson = $$"""
            {"viewer_id":"0","steam_id":0,"steam_session_ticket":"",
             "deck_no":1,"class_id":{{classId}},"leader_skin_id":{{leaderSkinId}},
             "is_random_leader_skin":false,"sleeve_id":{{sleeveId}},"deck_name":"Renamed",
             "is_delete":0,"deck_format":1}
            """;
        await client.PostAsync("/deck/update", JsonBody(updateJson));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var rows = await db.Decks.Where(d => d.Number == 1 && d.Format == Format.Rotation).ToListAsync();
        Assert.That(rows.Count, Is.EqualTo(1), "Update must not insert a duplicate row.");
        Assert.That(rows[0].Name, Is.EqualTo("Renamed"));
    }

    [Test]
    public async Task Update_with_is_delete_1_removes_the_slot()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, name: "Doomed");
        var (classId, sleeveId, leaderSkinId) = await FetchSeededIds(factory);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var deleteJson = $$"""
            {"viewer_id":"0","steam_id":0,"steam_session_ticket":"",
             "deck_no":1,"class_id":{{classId}},"leader_skin_id":{{leaderSkinId}},
             "is_random_leader_skin":false,"sleeve_id":{{sleeveId}},"deck_name":null,
             "is_delete":1,"deck_format":1}
            """;
        var response = await client.PostAsync("/deck/update", JsonBody(deleteJson));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var still = await db.Decks.AnyAsync(d => d.Number == 1 && d.Format == Format.Rotation);
        Assert.That(still, Is.False, "is_delete=1 should remove the row.");
    }

    [Test]
    public async Task Update_returns_refreshed_deck_list()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, name: "Existing");
        var (classId, sleeveId, leaderSkinId) = await FetchSeededIds(factory);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var updateJson = $$"""
            {"viewer_id":"0","steam_id":0,"steam_session_ticket":"",
             "deck_no":2,"class_id":{{classId}},"leader_skin_id":{{leaderSkinId}},
             "is_random_leader_skin":false,"sleeve_id":{{sleeveId}},"deck_name":"Second",
             "is_delete":0,"deck_format":1}
            """;
        var response = await client.PostAsync("/deck/update", JsonBody(updateJson));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var decks = doc.RootElement.GetProperty("user_deck_list");
        var realNames = Enumerable.Range(0, decks.GetArrayLength())
            .Select(i => decks[i])
            .Where(d => d.GetProperty("is_complete_deck").GetInt32() == 1)
            .Select(d => d.GetProperty("deck_name").GetString())
            .ToList();
        Assert.That(realNames, Is.EquivalentTo(new[] { "Existing", "Second" }),
            "/deck/update should hand back the full refreshed list, saving the client a follow-up.");
    }

    // ---- single-field mutations ----

    [Test]
    public async Task UpdateName_persists_and_returns_updated_user_deck()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1, name: "Old Name");
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"deck_name":"New Name","deck_format":1}""";
        var response = await client.PostAsync("/deck/update_name", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("user_deck").GetProperty("deck_name").GetString(),
            Is.EqualTo("New Name"));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var deck = await db.Decks.FirstAsync(d => d.Number == 1 && d.Format == Format.Rotation);
        Assert.That(deck.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public async Task UpdateSleeve_persists_and_returns_updated_user_deck()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        // Pick a different sleeve than the seed default to prove the change took.
        int sleeveId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            sleeveId = await db.Sleeves.OrderByDescending(s => s.Id).Select(s => s.Id).FirstAsync();
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deckNo":1,"sleeve_id":{{sleeveId}},"deckFormat":0}""";
        var response = await client.PostAsync("/deck/update_sleeve", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("user_deck").GetProperty("sleeve_id").GetInt32(),
            Is.EqualTo(sleeveId));
    }

    [Test]
    public async Task UpdateLeaderSkin_persists_and_clears_random_flag()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        int skinId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            skinId = await db.LeaderSkins.OrderByDescending(s => s.Id).Select(s => s.Id).FirstAsync();
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"leader_skin_id":{{skinId}},"deck_format":1}""";
        var response = await client.PostAsync("/deck/update_leader_skin", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var userDeck = doc.RootElement.GetProperty("user_deck");
        Assert.That(userDeck.GetProperty("leader_skin_id").GetInt32(), Is.EqualTo(skinId));
        Assert.That(userDeck.GetProperty("is_random_leader_skin").GetInt32(), Is.EqualTo(0),
            "Selecting a specific leader skin clears the random-skin flag.");
    }

    [Test]
    public async Task UpdateRandomLeaderSkin_picks_from_pool_and_persists()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        List<int> pool;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            pool = await db.LeaderSkins.OrderBy(s => s.Id).Take(3).Select(s => s.Id).ToListAsync();
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json =
            $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"deck_format":1,"leader_skin_id_list":[{{string.Join(',', pool)}}]}""";
        var response = await client.PostAsync("/deck/update_random_leader_skin", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var userDeck = doc.RootElement.GetProperty("user_deck");
        Assert.That(pool, Contains.Item(userDeck.GetProperty("leader_skin_id").GetInt32()),
            "Chosen skin must come from the supplied pool.");
        Assert.That(userDeck.GetProperty("is_random_leader_skin").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateRandomLeaderSkin_rejects_empty_pool_with_400()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"deck_format":1,"leader_skin_id_list":[]}""";
        var response = await client.PostAsync("/deck/update_random_leader_skin", JsonBody(json));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateOrder_returns_200()
    {
        // No persistence today (slot Number doubles as display order); just confirm the
        // endpoint round-trips so a future ordering schema doesn't silently regress 200→500.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 2);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_order":[2,1],"deck_format":1}""";
        var response = await client.PostAsync("/deck/update_order", JsonBody(json));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteDeckList_removes_listed_slots()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 1);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 2);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, 3);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no_list":[1,3],"deck_format":1}""";
        var response = await client.PostAsync("/deck/delete_deck_list", JsonBody(json));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var remaining = await db.Decks.Where(d => d.Format == Format.Rotation)
            .Select(d => d.Number).OrderBy(n => n).ToListAsync();
        Assert.That(remaining, Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public async Task SetDeckRedis_returns_200_for_authed_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"class_id":1}""";
        var response = await client.PostAsync("/deck/set_deck_redis", JsonBody(json));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
