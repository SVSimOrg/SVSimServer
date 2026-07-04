using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/reprinted-cards.json</c>. Source: <c>/load/index
/// data.reprinted_base_card_ids</c> (dict or list of card_ids).
/// </summary>
public sealed class ReprintedCardSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
}
