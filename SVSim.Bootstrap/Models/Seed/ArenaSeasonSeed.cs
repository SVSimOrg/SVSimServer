using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>
/// Mirrors <c>seeds/arena-season.json</c>. Singleton (id=1) holding the Take Two arena season.
/// <c>format_info</c> is a nested JSON object stored verbatim as the entity's <c>FormatInfo</c> jsonb.
/// </summary>
public sealed class ArenaSeasonSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("mode")] public int Mode { get; set; }
    [JsonPropertyName("enable")] public int Enable { get; set; }
    [JsonPropertyName("cost")] public ulong Cost { get; set; }
    [JsonPropertyName("rupy_cost")] public ulong RupyCost { get; set; }
    [JsonPropertyName("ticket_cost")] public int TicketCost { get; set; }
    [JsonPropertyName("is_join")] public bool IsJoin { get; set; }
    [JsonPropertyName("format_info")] public JsonElement FormatInfo { get; set; }
}
