using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class ArenaInfo
{
    [JsonPropertyName("mode")]
    [Key("mode")]
    public int Mode { get; set; }
    [JsonPropertyName("enable")]
    [Key("enable")]
    public int Enable { get; set; }
    [JsonPropertyName("cost")]
    [Key("cost")]
    public ulong Cost { get; set; }
    [JsonPropertyName("rupy_cost")]
    [Key("rupy_cost")]
    public ulong RupeeCost { get; set; }
    [JsonPropertyName("ticket_cost")]
    [Key("ticket_cost")]
    public int TicketCost { get; set; }
    [JsonPropertyName("is_join")]
    [Key("is_join")]
    public bool IsJoin { get; set; }
    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public ShopExpiryInfo? SalesPeriodInfo { get; set; }
    [JsonPropertyName("format_info")]
    [Key("format_info")]
    public ArenaFormatInfo? FormatInfo { get; set; }
}