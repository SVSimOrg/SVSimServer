using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// sealed_info on /mypage/index — current Sealed Arena season configuration. Consumed by
/// ArenaData.SetSealedMyPageResponseData (ArenaData.cs:59-65), which is Keys.Contains-guarded,
/// but post-parse UI almost certainly dereferences fields from the SealedMyPageResponseData
/// it builds. Since the user reclassified "Safe to omit" as a non-policy, we now always emit.
///
/// Prod-captured shape:
/// <code>
/// {"enable":1,"crystal_cost":600,"rupy_cost":600,"ticket_cost":4,"is_join":false,
///  "pack_info":[10032,10032,10031,10030,10029],"deck_using_num_min":30,"schedule_id":21,
///  "is_deck_code_maintenance":false,"sales_period_info":{"sales_period_series":33}}
/// </code>
/// </summary>
[MessagePackObject]
public class SealedInfo
{
    [JsonPropertyName("enable")]
    [Key("enable")]
    public int Enable { get; set; }

    [JsonPropertyName("crystal_cost")]
    [Key("crystal_cost")]
    public int CrystalCost { get; set; }

    [JsonPropertyName("rupy_cost")]
    [Key("rupy_cost")]
    public int RupyCost { get; set; }

    [JsonPropertyName("ticket_cost")]
    [Key("ticket_cost")]
    public int TicketCost { get; set; }

    [JsonPropertyName("is_join")]
    [Key("is_join")]
    public bool IsJoin { get; set; }

    /// <summary>Pack set ids used in this Sealed pool. Prod sends 5 entries (one per draft pack).</summary>
    [JsonPropertyName("pack_info")]
    [Key("pack_info")]
    public List<int> PackInfo { get; set; } = new();

    [JsonPropertyName("deck_using_num_min")]
    [Key("deck_using_num_min")]
    public int DeckUsingNumMin { get; set; }

    [JsonPropertyName("schedule_id")]
    [Key("schedule_id")]
    public int ScheduleId { get; set; }

    [JsonPropertyName("is_deck_code_maintenance")]
    [Key("is_deck_code_maintenance")]
    public bool IsDeckCodeMaintenance { get; set; }

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public SealedSalesPeriodInfo SalesPeriodInfo { get; set; } = new();
}
