using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.LeaderSkin;

// /leader_skin/products wire shape: `data` IS the per-series dict (no wrapping field).
// Per SkinPurchaseInfoTask.Parse line 31: `JsonData jsonData = base.ResponseData["data"];`
// then iterates positionally. Dict-keyed-by-series_id_string mirrors the prod capture exactly.
// The controller returns Dictionary<string, SkinSeriesDto> directly so `data` becomes that dict.

[MessagePackObject]
public class SkinSeriesDto
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }

    /// <summary>True when the viewer owns every product's skin in this series.</summary>
    [JsonPropertyName("is_completed")]
    [Key("is_completed")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("is_new")]
    [Key("is_new")]
    public bool IsNew { get; set; }

    /// <summary>Always emit — client unconditionally calls .ToInt() on this field.</summary>
    [JsonPropertyName("set_sales_status")]
    [Key("set_sales_status")]
    public int SetSalesStatus { get; set; }

    [JsonPropertyName("rewards")]
    [Key("rewards")]
    public SkinSeriesRewardsDto Rewards { get; set; } = new();

    /// <summary>Always emit — client reads this dict when set_sales_status != 0.</summary>
    [JsonPropertyName("set_prices")]
    [Key("set_prices")]
    public SkinSeriesSetPricesDto SetPrices { get; set; } = new();

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public List<object> SalesPeriodInfo { get; set; } = new();

    [JsonPropertyName("products")]
    [Key("products")]
    public List<SkinProductDto> Products { get; set; } = new();
}

[MessagePackObject]
public class SkinSeriesRewardsDto
{
    /// <summary>SkinSeriesPurchaseInfo.RewardStatus — 0=none, 1=not_got, 2=got.</summary>
    [JsonPropertyName("status")]
    [Key("status")]
    public int Status { get; set; }

    [JsonPropertyName("items")]
    [Key("items")]
    public List<SkinSeriesRewardItemDto> Items { get; set; } = new();
}

[MessagePackObject]
public class SkinSeriesRewardItemDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    public int RewardNumber { get; set; }
}

[MessagePackObject]
public class SkinSeriesSetPricesDto
{
    [JsonPropertyName("set_price_crystal")]
    [Key("set_price_crystal")]
    public int? SetPriceCrystal { get; set; }

    [JsonPropertyName("set_price_rupy")]
    [Key("set_price_rupy")]
    public int? SetPriceRupy { get; set; }

    [JsonPropertyName("set_price_ticket")]
    [Key("set_price_ticket")]
    public int? SetPriceTicket { get; set; }

    [JsonPropertyName("ticket_id")]
    [Key("ticket_id")]
    public long? TicketId { get; set; }
}

[MessagePackObject]
public class SkinProductDto
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }

    [JsonPropertyName("product_name")]
    [Key("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("introduction")]
    [Key("introduction")]
    public string Introduction { get; set; } = string.Empty;

    [JsonPropertyName("cv_name")]
    [Key("cv_name")]
    public string CvName { get; set; } = string.Empty;

    [JsonPropertyName("is_purchased")]
    [Key("is_purchased")]
    public bool IsPurchased { get; set; }

    [JsonPropertyName("sale")]
    [Key("sale")]
    public SkinProductSaleDto Sale { get; set; } = new();

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public List<object> SalesPeriodInfo { get; set; } = new();

    [JsonPropertyName("rewards")]
    [Key("rewards")]
    public List<SkinProductRewardDto> Rewards { get; set; } = new();
}

[MessagePackObject]
public class SkinProductSaleDto
{
    [JsonPropertyName("single_price_crystal")]
    [Key("single_price_crystal")]
    public int? SinglePriceCrystal { get; set; }

    [JsonPropertyName("single_price_rupy")]
    [Key("single_price_rupy")]
    public int? SinglePriceRupy { get; set; }

    [JsonPropertyName("single_price_ticket")]
    [Key("single_price_ticket")]
    public int? SinglePriceTicket { get; set; }

    [JsonPropertyName("ticket_number")]
    [Key("ticket_number")]
    public int? TicketNumber { get; set; }

    [JsonPropertyName("item_id")]
    [Key("item_id")]
    public long? ItemId { get; set; }
}

[MessagePackObject]
public class SkinProductRewardDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    public int RewardNumber { get; set; }

    [JsonPropertyName("is_owned")]
    [Key("is_owned")]
    public bool IsOwned { get; set; }
}
