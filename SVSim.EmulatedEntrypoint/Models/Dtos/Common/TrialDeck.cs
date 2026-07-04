using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// One archetype trial deck under <c>trial_deck_list</c> (DeckAttributeType.TrialDeck). Wire shape
/// from the 2026-05-29 main_story/get_deck_list capture. Distinct from build decks: carries
/// <c>deck_format</c> and no order_num/leader-skin-list. card_id_array only (no numbered card_id_N).
/// </summary>
[MessagePackObject]
public class TrialDeck
{
    [JsonPropertyName("deck_no")] [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("class_id")] [Key("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("sleeve_id")] [Key("sleeve_id")] public int SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")] [Key("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("deck_name")] [Key("deck_name")] public string DeckName { get; set; } = string.Empty;
    [JsonPropertyName("card_id_array")] [Key("card_id_array")] public List<long> CardIdArray { get; set; } = new();
    [JsonPropertyName("is_complete_deck")] [Key("is_complete_deck")] public int IsCompleteDeck { get; set; } = 1;
    [JsonPropertyName("restricted_card_exists")] [Key("restricted_card_exists")] public bool RestrictedCardExists { get; set; }
    [JsonPropertyName("is_available_deck")] [Key("is_available_deck")] public int IsAvailableDeck { get; set; } = 1;
    [JsonPropertyName("maintenance_card_ids")] [Key("maintenance_card_ids")] public List<long> MaintenanceCardIds { get; set; } = new();
    [JsonPropertyName("is_include_un_possession_card")] [Key("is_include_un_possession_card")] public bool IsIncludeUnPossessionCard { get; set; }
    [JsonPropertyName("deck_format")] [Key("deck_format")] public int DeckFormat { get; set; }
    [JsonPropertyName("is_recommend")] [Key("is_recommend")] public int IsRecommend { get; set; }
}
