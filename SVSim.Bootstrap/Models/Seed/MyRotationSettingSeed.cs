using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors <c>seeds/my-rotation-settings.json</c>. The extractor pre-joins
/// <c>my_rotation_info.{setting, reprinted_base_card_ids, restricted_base_card_id_list}</c> on
/// rotation_id into one flat list. <c>reprinted_card_ids</c> and <c>restricted_card_ids</c> are
/// pre-serialized JSON strings (verbatim from the wire) — the importer stores them verbatim.
/// </summary>
public sealed class MyRotationSettingSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("card_set_ids_csv")] public string CardSetIdsCsv { get; set; } = "";
    [JsonPropertyName("abilities_csv")] public string AbilitiesCsv { get; set; } = "";
    [JsonPropertyName("reprinted_card_ids")] public string ReprintedCardIds { get; set; } = "[]";
    [JsonPropertyName("restricted_card_ids")] public string RestrictedCardIds { get; set; } = "[]";
}
