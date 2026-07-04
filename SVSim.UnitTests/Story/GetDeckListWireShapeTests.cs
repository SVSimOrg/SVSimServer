using System.Text.Json;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;

namespace SVSim.UnitTests.Story;

public class GetDeckListWireShapeTests
{
    // Mirror Program.cs: keys come from [JsonPropertyName]; null values are dropped.
    private static readonly JsonSerializerOptions WireOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Test]
    public void BuildDeck_serializes_with_prod_wire_keys()
    {
        var deck = new BuildDeck
        {
            DeckNo = 701, OrderNum = 0, ClassId = 1, SleeveId = 3000011, LeaderSkinId = 1,
            EntryNo = 0, CreateDeckTime = null, DeckName = "Pure Devotion",
            CardIdArray = new() { 115141020, 114141020 },
            IsCompleteDeck = 1, IsAvailableDeck = 1, MaintenanceCardIds = new(), IsRecommend = 0,
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(deck, WireOptions));
        var root = doc.RootElement;

        // Every key the client's BuildDeck branch reads must be present with the right name.
        foreach (var key in new[] { "deck_no", "order_num", "class_id", "sleeve_id", "leader_skin_id",
                                     "entry_no", "deck_name", "card_id_array", "is_complete_deck",
                                     "is_available_deck", "maintenance_card_ids", "is_recommend" })
        {
            Assert.That(root.TryGetProperty(key, out _), Is.True, $"missing wire key: {key}");
        }
        Assert.That(root.GetProperty("deck_name").GetString(), Is.EqualTo("Pure Devotion"));
        Assert.That(root.GetProperty("card_id_array").GetArrayLength(), Is.EqualTo(2));
        // Numbered card_id_N keys are intentionally omitted.
        Assert.That(root.TryGetProperty("card_id_1", out _), Is.False);
    }

    [Test]
    public void TrialDeck_serializes_with_prod_wire_keys_including_deck_format()
    {
        var deck = new TrialDeck
        {
            DeckNo = 13001, ClassId = 1, SleeveId = 3000011, LeaderSkinId = 0,
            DeckName = "Tempo Forestcraft", CardIdArray = new() { 130141020 },
            IsCompleteDeck = 1, RestrictedCardExists = false, IsAvailableDeck = 1,
            MaintenanceCardIds = new(), IsIncludeUnPossessionCard = false, DeckFormat = 1, IsRecommend = 1,
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(deck, WireOptions));
        var root = doc.RootElement;

        foreach (var key in new[] { "deck_no", "class_id", "sleeve_id", "leader_skin_id", "deck_name",
                                     "card_id_array", "is_complete_deck", "restricted_card_exists",
                                     "is_available_deck", "maintenance_card_ids",
                                     "is_include_un_possession_card", "deck_format", "is_recommend" })
        {
            Assert.That(root.TryGetProperty(key, out _), Is.True, $"missing wire key: {key}");
        }
        Assert.That(root.GetProperty("deck_format").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public void GetDeckListResponse_default_deck_list_is_a_keyed_object()
    {
        var resp = new GetDeckListResponse();
        resp.DefaultDeckList["91"] = new SVSim.EmulatedEntrypoint.Models.Dtos.DefaultDeck
        {
            DeckNo = 91, ClassId = 1, SleeveId = 3000011, LeaderSkinId = 0, DeckName = "Default",
            CardIdArray = new() { 100111010 },
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(resp, WireOptions));
        var root = doc.RootElement;

        Assert.That(root.GetProperty("default_deck_list").ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(root.GetProperty("default_deck_list").GetProperty("91").GetProperty("class_id").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("build_deck_list").ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(root.GetProperty("trial_deck_list").ValueKind, Is.EqualTo(JsonValueKind.Array));
    }
}
