using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Deck;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>/practice/*</c>. The solo-battle subsystem is mostly stubbed (no XP,
/// no missions, no rewards) but the endpoints must still round-trip successfully or the
/// solo-play UI breaks before reaching the battle screen.
/// </summary>
public class PracticeControllerTests
{
    private const string BaseRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    // ToApi() converts internal Format -> wire deck_format int (Format.All -> 0, etc.).
    private static string DeckFormatRequestJson(Format f) =>
        $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_format":{{f.ToApi()}}}""";

    [Test]
    public async Task Info_returns_non_empty_opponent_array()
    {
        using var factory = new SVSimTestFactory();
        // Practice opponents are bootstrapped from seeds/practice-opponents.json into the
        // PracticeOpponents table — empty by default in tests, so seed first.
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/practice/info",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "/practice/info returns a bare array (no wrapper object) per spec.");
        Assert.That(doc.RootElement.GetArrayLength(), Is.GreaterThan(0));
        Assert.That(doc.RootElement[0].GetProperty("practice_id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Info_returns_empty_array_when_db_not_bootstrapped()
    {
        using var factory = new SVSimTestFactory();
        // Skip SeedGlobalsAsync — table is empty. /practice/info must still 200, not 500: the
        // client treats an empty array as "no opponents" and the practice menu just shows nothing.
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/practice/info",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeckList_returns_viewer_decks()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 1, name: "Rotation Deck");
        await factory.SeedDeckAsync(viewerId, Format.Unlimited, number: 1, name: "Unlimited Deck");

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/practice/deck_list",
            new StringContent(DeckFormatRequestJson(Format.All), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var rotation = doc.RootElement.GetProperty("user_deck_rotation");
        var unlimited = doc.RootElement.GetProperty("user_deck_unlimited");
        Assert.That(rotation.GetArrayLength(), Is.EqualTo(1));
        Assert.That(rotation[0].GetProperty("deck_name").GetString(), Is.EqualTo("Rotation Deck"));
        Assert.That(unlimited.GetArrayLength(), Is.EqualTo(1));
        Assert.That(unlimited[0].GetProperty("deck_name").GetString(), Is.EqualTo("Unlimited Deck"));
    }

    [Test]
    public async Task DeckList_card_id_array_contains_real_card_ids()
    {
        // Regression for the deck-include bug: PracticeController used to load the viewer
        // via GetViewerByShortUdid, which Includes Decks but NOT Decks.Cards.Card. The
        // DeckCard.Card navigation defaults to `new ShadowverseCardEntry()` (Id=0), so the
        // wire response shipped 40 zeros — which then NREs the client's SBattleLoad
        // (CardCreator returns null for id=0 → BattlePlayerBase.AddToDeck(null)). Asserts
        // a real card id round-trips end-to-end so the same .Include drop can't reappear.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        const long CardId = 900_123_456L;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.Cards.Add(new ShadowverseCardEntry { Id = CardId, Name = "Regression Card" });
            await db.SaveChangesAsync();
        }
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 1);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var repo = scope.ServiceProvider.GetRequiredService<IDeckRepository>();
            var card = await db.Cards.FirstAsync(c => c.Id == CardId);
            await repo.UpsertDeck(viewerId, Format.Rotation, 1,
                d => d.Cards = new List<DeckCard> { new() { Card = card, Count = 3 } });
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/practice/deck_list",
            new StringContent(DeckFormatRequestJson(Format.All), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var cards = doc.RootElement.GetProperty("user_deck_rotation")[0].GetProperty("card_id_array");
        Assert.That(cards.GetArrayLength(), Is.EqualTo(3), "DeckCard.Count should expand into Count copies in card_id_array");
        for (int i = 0; i < 3; i++)
        {
            Assert.That(cards[i].GetInt64(), Is.EqualTo(CardId), "card_id must round-trip — Includes are dropping DeckCard.Card");
        }
    }

    [Test]
    public async Task DeckList_exposes_the_eight_default_decks()
    {
        // Prod's practice/deck_list returns the same shape as /deck/info, including the 8 per-class
        // starter decks under default_deck_list (keyed by deck_no "91".."98"). Without them, a fresh
        // account has no decks to pick and can't start a practice match.
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();        // imports the 8 default decks
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/practice/deck_list",
            new StringContent(DeckFormatRequestJson(Format.All), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var defaults = doc.RootElement.GetProperty("default_deck_list");
        Assert.That(defaults.ValueKind, Is.EqualTo(JsonValueKind.Object));
        foreach (var key in new[] { "91", "92", "93", "94", "95", "96", "97", "98" })
        {
            Assert.That(defaults.TryGetProperty(key, out _), Is.True, $"missing default deck {key}");
        }
        Assert.That(defaults.GetProperty("91").GetProperty("class_id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task DeckList_empty_when_viewer_has_none()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/practice/deck_list",
            new StringContent(DeckFormatRequestJson(Format.All), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("user_deck_rotation").GetArrayLength(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("user_deck_unlimited").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Start_returns_200()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/practice/start",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Finish_win_grants_class_xp_and_persists_level_up()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // recoveryData is an opaque JSON blob serialized to string by the client; the server
        // is supposed to accept it without validation. Anything goes.
        // deck_format:1 = Format.Rotation on the wire. The controller ignores the field today
        // (practice is per-format upstream), but sending a coherent wire code keeps the test
        // intent clean if Finish ever starts validating it.
        var finishJson =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"is_win":1,"evolve_count":2,"total_turn":5,"enemy_class_id":3,"difficulty":1,"deck_format":1,"class_id":1,"recovery_data":"{\"opaque\":\"blob\"}"}""";

        var response = await client.PostAsync("/practice/finish",
            new StringContent(finishJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        // BattleXpConfig.XpPerWin default = 200. classexp.csv seeds L1=50, L2=150 → 200 XP
        // crosses L1 (spends 50, level→2, exp=150) then L2 (spends 150, level→3, exp=0).
        Assert.That(doc.RootElement.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(200));
        Assert.That(doc.RootElement.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("class_level").GetInt32(), Is.EqualTo(3));
        Assert.That(doc.RootElement.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));

        // Persistence check — reload viewer and confirm the ViewerClassData row moved.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.Classes).ThenInclude(c => c.Class)
            .FirstAsync(x => x.Id == viewerId);
        var cls1 = v.Classes.Single(c => c.Class.Id == 1);
        Assert.That(cls1.Level, Is.EqualTo(3));
        Assert.That(cls1.Exp, Is.EqualTo(0));
    }

    [Test]
    public async Task Finish_loss_grants_default_loss_xp()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var finishJson =
            """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_no":1,"is_win":0,"evolve_count":0,"total_turn":3,"enemy_class_id":3,"difficulty":1,"deck_format":1,"class_id":1,"recovery_data":"{}"}""";

        var response = await client.PostAsync("/practice/finish",
            new StringContent(finishJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        // BattleXpConfig.XpPerLoss default = 50. classexp.csv L1=50 → 50 XP exactly meets
        // the threshold: Level=2, Exp=0.
        Assert.That(doc.RootElement.GetProperty("get_class_experience").GetInt32(), Is.EqualTo(50));
        Assert.That(doc.RootElement.GetProperty("class_experience").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("class_level").GetInt32(), Is.EqualTo(2));
    }
}
