using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One of the eight "starter" decks (one per class), as surfaced under
/// <c>/deck/info data.default_deck_list</c>. Wire shape derived from 2026-05-23 prod capture.
/// Used by the client both as new-account defaults and as the source for "use default deck".
/// </summary>
[MessagePackObject]
public class DefaultDeck
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")]
    public int DeckNo { get; set; }

    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")]
    public long SleeveId { get; set; }

    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }

    [JsonPropertyName("deck_name")]
    [Key("deck_name")]
    public string DeckName { get; set; } = string.Empty;

    /// <summary>40 card_id values — same card may repeat (max 3 per card per Shadowverse rules).</summary>
    [JsonPropertyName("card_id_array")]
    [Key("card_id_array")]
    public List<long> CardIdArray { get; set; } = new();

    /// <summary>0/1. Client reads via GetJsonBool(default true) in DeckData.Initialize. Prod always sends 1 for the 8 starter decks.</summary>
    [JsonPropertyName("is_complete_deck")]
    [Key("is_complete_deck")]
    public int IsCompleteDeck { get; set; } = 1;

    /// <summary>0/1. Read by downstream deck-edit UI (not by DeckData.Initialize itself). Prod always sends 1.</summary>
    [JsonPropertyName("is_available_deck")]
    [Key("is_available_deck")]
    public int IsAvailableDeck { get; set; } = 1;

    /// <summary>Card ids currently under maintenance (disabled). Empty for the 8 starter decks in prod.</summary>
    [JsonPropertyName("maintenance_card_ids")]
    [Key("maintenance_card_ids")]
    public List<long> MaintenanceCardIds { get; set; } = new();
}
