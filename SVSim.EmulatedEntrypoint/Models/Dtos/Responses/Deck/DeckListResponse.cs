using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;

/// <summary>
/// Shape consumed by <c>DeckGroupListData(jsonData, format)</c>. Spec at
/// <c>docs/api-spec/endpoints/post-login/deck-info.md</c> only enumerates <c>maintenance_card_list</c>
/// and <c>user_deck_list</c> explicitly (with <c>[k: string]: unknown</c> for the rest); the 2026-05-23
/// prod capture filled in the gap — <c>default_deck_list</c>, <c>user_leader_skin_setting_list</c>,
/// and <c>trial_deck_list</c> are all present and sourced from globals.
/// </summary>
[MessagePackObject]
public class DeckListResponse
{
    [JsonPropertyName("maintenance_card_list")]
    [Key("maintenance_card_list")] public List<long> MaintenanceCardList { get; set; } = new();

    /// <summary>
    /// Single-format viewer decks. Emitted when the request specified a specific format
    /// (e.g. Rotation, Unlimited) — mutually exclusive with the per-format keys below.
    /// </summary>
    [JsonPropertyName("user_deck_list")]
    [Key("user_deck_list")] public List<UserDeck>? UserDeckList { get; set; }

    /// <summary>
    /// Per-format viewer decks. Emitted when the request specified All format (deck_format=0).
    /// Prod's <c>DeckListUtility.ParseDeckInfoResponceData</c> All-format branch only walks these
    /// per-format keys (not user_deck_list), so the controller swaps shape based on the request.
    /// The PreRotation / Crossover / Avatar siblings exist in client code but prod omits them
    /// for fresh viewers; we mirror that omission.
    /// </summary>
    [JsonPropertyName("user_deck_rotation")]
    [Key("user_deck_rotation")] public List<UserDeck>? UserDeckRotation { get; set; }

    [JsonPropertyName("user_deck_unlimited")]
    [Key("user_deck_unlimited")] public List<UserDeck>? UserDeckUnlimited { get; set; }

    [JsonPropertyName("user_deck_my_rotation")]
    [Key("user_deck_my_rotation")] public List<UserDeck>? UserDeckMyRotation { get; set; }

    /// <summary>
    /// Global starter decks, keyed by deck_no as string (prod ids 91-98 — one per class).
    /// </summary>
    [JsonPropertyName("default_deck_list")]
    [Key("default_deck_list")] public Dictionary<string, DefaultDeck> DefaultDeckList { get; set; } = new();

    /// <summary>
    /// Per-class leader skin setting (active skin id) for the requesting viewer, keyed by
    /// class_id as string.
    /// </summary>
    [JsonPropertyName("user_leader_skin_setting_list")]
    [Key("user_leader_skin_setting_list")] public Dictionary<string, UserLeaderSkinSetting> UserLeaderSkinSettingList { get; set; } = new();

    /// <summary>
    /// Trial / archetype decks. Prod emits this on <c>/deck/info</c> (All format) but OMITS the key
    /// entirely on <c>/deck/my_list</c> (specific-format) — controller mirrors that asymmetry by
    /// leaving this null on specific-format responses. Emitted EMPTY on /deck/info (matches the
    /// 2026-05-23 prod capture); story/get_deck_list is where trial decks are actually populated.
    /// </summary>
    [JsonPropertyName("trial_deck_list")]
    [Key("trial_deck_list")] public List<TrialDeck>? TrialDeckList { get; set; }
}
