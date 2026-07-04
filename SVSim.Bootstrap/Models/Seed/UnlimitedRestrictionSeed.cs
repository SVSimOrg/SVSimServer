using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/unlimited-restrictions.json</c>. Source: <c>/load/index
/// data.unlimited_restricted_base_card_id_list</c> (dict {card_id: restriction_value}).
/// </summary>
public sealed class UnlimitedRestrictionSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
    [JsonPropertyName("restriction_value")] public int RestrictionValue { get; set; }
}
