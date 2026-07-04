using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors one entry of <c>seeds/maintenance-cards.json</c>. Source: <c>/load/index
/// data.maintenance_card_list</c> (array of card_ids; usually empty).
/// </summary>
public sealed class MaintenanceCardSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
}
