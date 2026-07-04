using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class GetDeckListRequest : BaseRequest
{
    [JsonPropertyName("story_id")]
    [Key("story_id")]
    public int StoryId { get; set; }
}

[MessagePackObject]
public class GetDeckListResponse
{
    [JsonPropertyName("user_deck_rotation")]
    [Key("user_deck_rotation")]
    public List<UserDeck> UserDeckRotation { get; set; } = new();

    [JsonPropertyName("user_deck_unlimited")]
    [Key("user_deck_unlimited")]
    public List<UserDeck> UserDeckUnlimited { get; set; } = new();

    [JsonPropertyName("maintenance_card_list")]
    [Key("maintenance_card_list")]
    public List<long> MaintenanceCardList { get; set; } = new();

    [JsonPropertyName("build_deck_list")]
    [Key("build_deck_list")]
    public List<BuildDeck> BuildDeckList { get; set; } = new();

    [JsonPropertyName("trial_deck_list")]
    [Key("trial_deck_list")]
    public List<TrialDeck> TrialDeckList { get; set; } = new();

    /// <summary>Global starter decks, keyed by deck_no string (prod ids 91-98, one per class).</summary>
    [JsonPropertyName("default_deck_list")]
    [Key("default_deck_list")]
    public Dictionary<string, DefaultDeck> DefaultDeckList { get; set; } = new();
}

/// <summary>
/// One named prebuilt story deck under <c>build_deck_list</c> (DeckAttributeType.BuildDeck). Wire
/// shape from the 2026-05-29 capture. Emits card_id_array only — the numbered card_id_1..40 keys
/// prod also sends are omitted (default/trial entries omit them and parse fine).
/// </summary>
[MessagePackObject]
public class BuildDeck
{
    [JsonPropertyName("deck_no")] [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("order_num")] [Key("order_num")] public int OrderNum { get; set; }
    [JsonPropertyName("class_id")] [Key("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("sleeve_id")] [Key("sleeve_id")] public int SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")] [Key("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("entry_no")] [Key("entry_no")] public int EntryNo { get; set; }
    [JsonPropertyName("create_deck_time")] [Key("create_deck_time")] public DateTime? CreateDeckTime { get; set; }
    [JsonPropertyName("deck_name")] [Key("deck_name")] public string DeckName { get; set; } = string.Empty;
    [JsonPropertyName("card_id_array")] [Key("card_id_array")] public List<long> CardIdArray { get; set; } = new();
    [JsonPropertyName("is_complete_deck")] [Key("is_complete_deck")] public int IsCompleteDeck { get; set; } = 1;
    [JsonPropertyName("is_available_deck")] [Key("is_available_deck")] public int IsAvailableDeck { get; set; } = 1;
    [JsonPropertyName("maintenance_card_ids")] [Key("maintenance_card_ids")] public List<long> MaintenanceCardIds { get; set; } = new();
    [JsonPropertyName("is_recommend")] [Key("is_recommend")] public int IsRecommend { get; set; }
}
