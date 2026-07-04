using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/spot-cards.json</c>. Source: <c>/load/index data.spot_cards</c>
/// — extractor reshapes the wire dict {card_id: cost} into a list of {card_id, cost} rows.
/// </summary>
public sealed class SpotCardSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
    [JsonPropertyName("cost")] public int Cost { get; set; }
}
