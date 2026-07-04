using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PackSeed
{
    [JsonPropertyName("parent_gacha_id")] public int ParentGachaId { get; set; }
    [JsonPropertyName("base_pack_id")] public int BasePackId { get; set; }
    [JsonPropertyName("gacha_type")] public int GachaType { get; set; }
    [JsonPropertyName("pack_category")] public int PackCategory { get; set; }
    [JsonPropertyName("poster_type")] public int PosterType { get; set; }
    [JsonPropertyName("commence_date")] public string CommenceDate { get; set; } = "";
    [JsonPropertyName("complete_date")] public string CompleteDate { get; set; } = "";
    [JsonPropertyName("sleeve_id")] public int SleeveId { get; set; }
    [JsonPropertyName("special_sleeve_id")] public int SpecialSleeveId { get; set; }
    [JsonPropertyName("override_draw_effect_pack_id")] public int OverrideDrawEffectPackId { get; set; }
    [JsonPropertyName("override_ui_effect_pack_id")] public int OverrideUiEffectPackId { get; set; }
    [JsonPropertyName("gacha_detail")] public string GachaDetail { get; set; } = "";
    [JsonPropertyName("is_hide")] public bool IsHide { get; set; }
    [JsonPropertyName("is_new")] public bool IsNew { get; set; }
    [JsonPropertyName("is_pre_release")] public bool IsPreRelease { get; set; }
    [JsonPropertyName("open_count_limit")] public int OpenCountLimit { get; set; }
    [JsonPropertyName("sales_period_time")] public string? SalesPeriodTime { get; set; }
    [JsonPropertyName("gacha_point")] public PackGachaPointSeed? GachaPoint { get; set; }
    [JsonPropertyName("child_gachas")] public List<PackChildGachaSeed> ChildGachas { get; set; } = new();
    [JsonPropertyName("banners")] public List<PackBannerSeed> Banners { get; set; } = new();
    [JsonPropertyName("is_enabled")] public bool IsEnabled { get; set; } = true;
}

public sealed class PackGachaPointSeed
{
    [JsonPropertyName("exchangeable_point")] public int ExchangeablePoint { get; set; }
    [JsonPropertyName("increase_gacha_point")] public int IncreaseGachaPoint { get; set; }
}

public sealed class PackChildGachaSeed
{
    [JsonPropertyName("gacha_id")] public int GachaId { get; set; }
    [JsonPropertyName("type_detail")] public int TypeDetail { get; set; }
    [JsonPropertyName("cost")] public int Cost { get; set; }
    [JsonPropertyName("card_count")] public int CardCount { get; set; } = 8;
    [JsonPropertyName("item_id")] public long? ItemId { get; set; }
    [JsonPropertyName("is_daily_single")] public bool IsDailySingle { get; set; }
    [JsonPropertyName("override_increase_gacha_point")] public int OverrideIncreaseGachaPoint { get; set; }
    [JsonPropertyName("purchase_limit_count")] public int PurchaseLimitCount { get; set; }
    [JsonPropertyName("daily_free_gacha_count")] public int DailyFreeGachaCount { get; set; }
    [JsonPropertyName("free_gacha_campaign_id")] public int? FreeGachaCampaignId { get; set; }
    [JsonPropertyName("campaign_name")] public string? CampaignName { get; set; }
}

public sealed class PackBannerSeed
{
    [JsonPropertyName("banner_name")] public string BannerName { get; set; } = "";
    [JsonPropertyName("dialog_title")] public string DialogTitle { get; set; } = "";
}
