using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Sleeve;

/// <summary>
/// /sleeve/info response. Wire shape: <c>{sleeve_list: {<series_id_str>: SleeveSeriesDto}}</c>.
/// Dict-keyed (not array) to match the prod capture exactly — LitJson's numeric indexer in
/// <c>SleevePurchaseInfoTask.Parse()</c> iterates dict values by inserted order, so either
/// shape would work, but mirroring the wire avoids surprise.
/// </summary>
[MessagePackObject]
public class SleeveInfoResponse
{
    [JsonPropertyName("sleeve_list")]
    [Key("sleeve_list")]
    public Dictionary<string, SleeveSeriesDto> SleeveList { get; set; } = new();
}

[MessagePackObject]
public class SleeveSeriesDto
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }

    [JsonPropertyName("is_new")]
    [Key("is_new")]
    public bool IsNew { get; set; }

    /// <summary>Dict keyed by product_id string — same iteration convention as sleeve_list.</summary>
    [JsonPropertyName("product_info")]
    [Key("product_info")]
    public Dictionary<string, SleeveProductDto> ProductInfo { get; set; } = new();
}

[MessagePackObject]
public class SleeveProductDto
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    /// <summary>SystemText key (e.g. "sleeve_138") — client resolves via GetSleeveProductText.</summary>
    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rewards")]
    [Key("rewards")]
    public List<SleeveProductRewardDto> Rewards { get; set; } = new();

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public List<object> SalesPeriodInfo { get; set; } = new();   // always [] in v1

    [JsonPropertyName("is_purchased_product")]
    [Key("is_purchased_product")]
    public bool IsPurchasedProduct { get; set; }

    [JsonPropertyName("price_crystal")]
    [Key("price_crystal")]
    public int? PriceCrystal { get; set; }

    [JsonPropertyName("price_rupy")]
    [Key("price_rupy")]
    public int? PriceRupy { get; set; }
}

[MessagePackObject]
public class SleeveProductRewardDto
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
