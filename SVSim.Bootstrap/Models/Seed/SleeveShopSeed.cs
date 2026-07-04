using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class SleeveShopSeriesSeed
{
    [JsonPropertyName("series_id")] public int SeriesId { get; set; }
    [JsonPropertyName("is_new")] public bool IsNew { get; set; }
    [JsonPropertyName("products")] public List<SleeveShopProductSeed> Products { get; set; } = new();
}

public sealed class SleeveShopProductSeed
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("name_key")] public string NameKey { get; set; } = "";
    [JsonPropertyName("price_crystal")] public int? PriceCrystal { get; set; }
    [JsonPropertyName("price_rupy")] public int? PriceRupy { get; set; }
    [JsonPropertyName("rewards")] public List<SleeveShopRewardSeed> Rewards { get; set; } = new();
}

public sealed class SleeveShopRewardSeed
{
    [JsonPropertyName("order_index")] public int OrderIndex { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
}
