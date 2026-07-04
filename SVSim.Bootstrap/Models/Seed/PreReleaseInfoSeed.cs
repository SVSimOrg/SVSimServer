using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors <c>seeds/pre-release-info.json</c>. Singleton (id=1). Card-id lists are kept as raw
/// JSON elements so they round-trip verbatim into the entity's jsonb columns.
/// </summary>
public sealed class PreReleaseInfoSeed
{
    [JsonPropertyName("pre_release_id")] public string PreReleaseId { get; set; } = "";
    [JsonPropertyName("next_card_set_id")] public string NextCardSetId { get; set; } = "";
    [JsonPropertyName("start_time")] public string StartTime { get; set; } = "";
    [JsonPropertyName("end_time")] public string EndTime { get; set; } = "";
    [JsonPropertyName("display_end_time")] public string DisplayEndTime { get; set; } = "";
    [JsonPropertyName("free_match_start_time")] public string FreeMatchStartTime { get; set; } = "";
    [JsonPropertyName("card_master_id")] public int CardMasterId { get; set; }
    [JsonPropertyName("default_card_master_id")] public string DefaultCardMasterId { get; set; } = "";
    [JsonPropertyName("pre_release_card_master_id")] public string PreReleaseCardMasterId { get; set; } = "";
    [JsonPropertyName("is_pre_rotation_free_match_term")] public bool IsPreRotationFreeMatchTerm { get; set; }
    [JsonPropertyName("rotation_card_set_id_list")] public JsonElement RotationCardSetIdList { get; set; }
    [JsonPropertyName("reprinted_base_card_ids")] public JsonElement ReprintedBaseCardIds { get; set; }
    [JsonPropertyName("latest_reprinted_base_card_ids")] public JsonElement LatestReprintedBaseCardIds { get; set; }
}
