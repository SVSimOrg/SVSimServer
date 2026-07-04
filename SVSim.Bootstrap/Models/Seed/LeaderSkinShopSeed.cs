using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class LeaderSkinShopSeriesSeed
{
    [JsonPropertyName("series_id")] public int SeriesId { get; set; }
    [JsonPropertyName("is_new")] public bool IsNew { get; set; }
    [JsonPropertyName("set_sales_status")] public int SetSalesStatus { get; set; }
    [JsonPropertyName("set_price_crystal")] public int? SetPriceCrystal { get; set; }
    [JsonPropertyName("set_price_rupy")] public int? SetPriceRupy { get; set; }
    [JsonPropertyName("set_price_ticket")] public int? SetPriceTicket { get; set; }
    [JsonPropertyName("set_price_ticket_id")] public long? SetPriceTicketId { get; set; }
    [JsonPropertyName("set_completion_rewards")] public List<LeaderSkinShopRewardSeed> SetCompletionRewards { get; set; } = new();
    [JsonPropertyName("products")] public List<LeaderSkinShopProductSeed> Products { get; set; } = new();
}

public sealed class LeaderSkinShopProductSeed
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("product_name_key")] public string ProductNameKey { get; set; } = "";
    [JsonPropertyName("introduction_key")] public string IntroductionKey { get; set; } = "";
    [JsonPropertyName("cv_name_key")] public string CvNameKey { get; set; } = "";
    [JsonPropertyName("single_price_crystal")] public int? SinglePriceCrystal { get; set; }
    [JsonPropertyName("single_price_rupy")] public int? SinglePriceRupy { get; set; }
    [JsonPropertyName("single_price_ticket")] public int? SinglePriceTicket { get; set; }
    [JsonPropertyName("ticket_number")] public int? TicketNumber { get; set; }
    [JsonPropertyName("ticket_item_id")] public long? TicketItemId { get; set; }
    [JsonPropertyName("rewards")] public List<LeaderSkinShopRewardSeed> Rewards { get; set; } = new();
}

public sealed class LeaderSkinShopRewardSeed
{
    [JsonPropertyName("order_index")] public int OrderIndex { get; set; }
    [JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
}
