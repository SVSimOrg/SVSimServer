using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class PackChildGachaDto
{
    [JsonPropertyName("gacha_id")]
    [Key("gacha_id")]
    public int GachaId { get; set; }

    [JsonPropertyName("type_detail")]
    [Key("type_detail")]
    public int TypeDetail { get; set; }

    [JsonPropertyName("cost")]
    [Key("cost")]
    public int Cost { get; set; }

    [JsonPropertyName("count")]
    [Key("count")]
    public int Count { get; set; } = 8;

    /// <summary>Stringified on the wire when present (prod sends "10001" not 10001).</summary>
    [JsonPropertyName("item_id")]
    [Key("item_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ItemId { get; set; }

    [JsonPropertyName("item_number")]
    [Key("item_number")]
    public int ItemNumber { get; set; }

    [JsonPropertyName("is_daily_single")]
    [Key("is_daily_single")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsDailySingle { get; set; }

    [JsonPropertyName("override_increase_gacha_point")]
    [Key("override_increase_gacha_point")]
    public string OverrideIncreaseGachaPoint { get; set; } = "0";

    /// <summary>Set on type_detail=10 free children only. Emitted as string on the wire.</summary>
    [JsonPropertyName("campaign_name")]
    [Key("campaign_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CampaignName { get; set; }

    /// <summary>Lifetime claim cap for this campaign. Stringified on the wire ("1") when present.</summary>
    [JsonPropertyName("purchase_limit_count")]
    [Key("purchase_limit_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PurchaseLimitCount { get; set; }

    /// <summary>Per-UTC-day claim cap for this campaign. Stringified on the wire ("1") when present.</summary>
    [JsonPropertyName("daily_free_gacha_count")]
    [Key("daily_free_gacha_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DailyFreeGachaCount { get; set; }

    /// <summary>Campaign id used to scope claim quotas server-side.</summary>
    [JsonPropertyName("free_gacha_campaign_id")]
    [Key("free_gacha_campaign_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FreeGachaCampaignId { get; set; }
}
