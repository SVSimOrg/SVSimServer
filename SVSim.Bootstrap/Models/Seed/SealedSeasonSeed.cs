using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class SealedSeasonSeed
{
    [JsonPropertyName("id")] public int Id { get; set; } = 1;
    [JsonPropertyName("enable")] public int Enable { get; set; }
    [JsonPropertyName("crystal_cost")] public int CrystalCost { get; set; }
    [JsonPropertyName("rupy_cost")] public int RupyCost { get; set; }
    [JsonPropertyName("ticket_cost")] public int TicketCost { get; set; }
    [JsonPropertyName("deck_using_num_min")] public int DeckUsingNumMin { get; set; }
    [JsonPropertyName("schedule_id")] public int ScheduleId { get; set; }
    [JsonPropertyName("is_join")] public bool IsJoin { get; set; }
    [JsonPropertyName("is_deck_code_maintenance")] public bool IsDeckCodeMaintenance { get; set; }
    [JsonPropertyName("pack_info")] public JsonElement PackInfo { get; set; }
    [JsonPropertyName("sales_period_info")] public JsonElement SalesPeriodInfo { get; set; }
}
