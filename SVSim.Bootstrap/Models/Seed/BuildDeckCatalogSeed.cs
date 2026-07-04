using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class BuildDeckCatalogSeed
{
    [JsonPropertyName("series_id")] public int SeriesId { get; set; }
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("is_new")] public bool IsNew { get; set; }
    [JsonPropertyName("series_rewards")] public List<BuildDeckSeriesRewardSeed> SeriesRewards { get; set; } = new();
    [JsonPropertyName("products")] public List<BuildDeckProductSeed> Products { get; set; } = new();
}

public sealed class BuildDeckSeriesRewardSeed
{
    [JsonPropertyName("tier_index")] public int TierIndex { get; set; }
    [JsonPropertyName("item_index")] public int ItemIndex { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("message_id")] public int MessageId { get; set; }
}

public sealed class BuildDeckProductSeed
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("leader_id")] public int LeaderId { get; set; }
    [JsonPropertyName("deck_code")] public string DeckCode { get; set; } = "";
    [JsonPropertyName("product_name")] public string ProductName { get; set; } = "";
    [JsonPropertyName("featured_card_id")] public long FeaturedCardId { get; set; }
    [JsonPropertyName("purchase_num_max")] public int PurchaseNumMax { get; set; }
    [JsonPropertyName("intro_price_crystal")] public int? IntroPriceCrystal { get; set; }
    [JsonPropertyName("regular_price_crystal")] public int? RegularPriceCrystal { get; set; }
    [JsonPropertyName("intro_price_rupy")] public int? IntroPriceRupy { get; set; }
    [JsonPropertyName("regular_price_rupy")] public int? RegularPriceRupy { get; set; }
    [JsonPropertyName("rewards")] public List<BuildDeckProductRewardSeed> Rewards { get; set; } = new();
}

public sealed class BuildDeckProductRewardSeed
{
    [JsonPropertyName("reward_index")] public int RewardIndex { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [JsonPropertyName("message_id")] public int MessageId { get; set; }
}
