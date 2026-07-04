using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/loading-exclusion-cards.json</c>. Source: <c>/load/index
/// data.loading_exclusion_card_list</c> (array of card_ids).
/// </summary>
public sealed class LoadingExclusionCardSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
}
