using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// A deck belonging to a user.
/// </summary>
[MessagePackObject]
public class UserDeck
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")]
    public int DeckNumber { get; set; }
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }
    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")]
    public int SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }
    [JsonPropertyName("deck_name")]
    [Key("deck_name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("card_id_array")]
    [Key("card_id_array")]
    public List<long> Cards { get; set; } = new List<long>();
    [JsonPropertyName("is_complete_deck")]
    [Key("is_complete_deck")]
    public int IsCompleteDeck { get; set; }
    [JsonPropertyName("restricted_card_exists")]
    [Key("restricted_card_exists")]
    public bool RestrictedCardExists { get; set; }
    [JsonPropertyName("is_available_deck")]
    [Key("is_available_deck")]
    public int IsAvailable { get; set; }
    [JsonPropertyName("maintenance_card_ids")]
    [Key("maintenance_card_ids")]
    public List<long> MaintenanceCards { get; set; } = new List<long>();
    [JsonPropertyName("is_include_un_possession_card")]
    [Key("is_include_un_possession_card")]
    public bool IncludesNonCollectibleCards { get; set; }
    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")]
    public int IsRandomLeaderSkin { get; set; }
    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")]
    public List<int> LeaderSkinIds { get; set; } = new List<int> { 0 };
    [JsonPropertyName("order_num")]
    [Key("order_num")]
    public int Order { get; set; }
    [JsonPropertyName("create_deck_time")]
    [Key("create_deck_time")]
    public DateTime? DeckCreateTime { get; set; }

    /// <summary>
    /// MyRotation period id. Emitted only for Format.MyRotation decks; the client's
    /// DeckData.Initialize reads it via GetValueOrDefault("rotation_id", null) and resolves
    /// against Data.MyRotationAllInfo. A MyRotation deck without this field crashes the
    /// deck-detail dialog inside DeckData.CreateMyRotationClassName (info.LastPackText on null).
    /// </summary>
    [JsonPropertyName("rotation_id")]
    [Key("rotation_id")]
    public string? RotationId { get; set; }

    /// <summary>
    /// Empty placeholder matching the wire shape prod uses to pad deck-list responses up to the
    /// per-format cap. The client's <c>DeckUI.DeckViewData.CreateDeckViewList</c> converts the
    /// first entry whose <c>card_id_array</c> is empty into the "New Deck" tile, so at least one
    /// of these must appear in any list the player can edit.
    /// </summary>
    public static UserDeck CreateEmptySlot(int deckNo) => new()
    {
        DeckNumber = deckNo,
        ClassId = 1,
        SleeveId = 3000011,
        LeaderSkinId = 0,
        Name = string.Empty,
        Cards = new(),
        IsCompleteDeck = 0,
        RestrictedCardExists = false,
        IsAvailable = 1,
        MaintenanceCards = new(),
        IncludesNonCollectibleCards = false,
        IsRandomLeaderSkin = 0,
        LeaderSkinIds = new() { 0 },
        Order = 0,
        DeckCreateTime = null,
    };

    public UserDeck(ShadowverseDeckEntry deck)
    {
        this.DeckNumber = deck.Number;
        this.ClassId = deck.Class.Id;
        this.LeaderSkinId = deck.LeaderSkin.Id;
        this.SleeveId = deck.Sleeve.Id;
        this.Name = deck.Name;
        this.Cards = deck.Cards.SelectMany(card => Enumerable.Range(0, card.Count).Select(count => card.Card.Id))
            .ToList();
        this.IsRandomLeaderSkin = deck.RandomLeaderSkin ? 1 : 0;
        this.Order = deck.Number;
        this.DeckCreateTime = deck.DateCreated;
        this.RotationId = deck.MyRotationId;
        
        //TODO probably want to calc some of these on demand
        this.IsCompleteDeck = 1;
        this.RestrictedCardExists = false;
        this.IsAvailable = 1;
        this.MaintenanceCards = new();
        this.IncludesNonCollectibleCards = false;
        
    }

    public UserDeck()
    {
    }
}