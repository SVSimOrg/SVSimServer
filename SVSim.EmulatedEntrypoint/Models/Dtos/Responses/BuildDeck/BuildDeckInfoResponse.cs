using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BuildDeck;

// /build_deck/info wire shape: the controller returns `List<BuildDeckSeriesDto>` directly so
// `data` becomes a bare array `[{series_id:...},...]`. The client iterates `data` via numeric
// indexer; a wrapper object like `{series_list:[...]}` would put the array one level deeper
// and break the iteration. There is no BuildDeckInfoResponse wrapper type — the response IS
// the series list.

[MessagePackObject]
public class BuildDeckSeriesDto
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }

    [JsonPropertyName("order_id")]
    [Key("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("is_new")]
    [Key("is_new")]
    public bool IsNew { get; set; }

    [JsonPropertyName("products")]
    [Key("products")]
    public List<BuildDeckProductDto> Products { get; set; } = new();

    [JsonPropertyName("series_rewards")]
    [Key("series_rewards")]
    public List<BuildDeckSeriesRewardTierDto> SeriesRewards { get; set; } = new();
}

[MessagePackObject]
public class BuildDeckProductDto
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("product_name")]
    [Key("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("leader_id")]
    [Key("leader_id")]
    public int LeaderId { get; set; }

    [JsonPropertyName("deck_code")]
    [Key("deck_code")]
    public string DeckCode { get; set; } = string.Empty;

    [JsonPropertyName("featured_card_id")]
    [Key("featured_card_id")]
    public long FeaturedCardId { get; set; }

    [JsonPropertyName("purchase_num_max")]
    [Key("purchase_num_max")]
    public int PurchaseNumMax { get; set; }

    [JsonPropertyName("purchase_num_current")]
    [Key("purchase_num_current")]
    public int PurchaseNumCurrent { get; set; }

    [JsonPropertyName("is_first_price")]
    [Key("is_first_price")]
    public bool IsFirstPrice { get; set; }

    [JsonPropertyName("rewards")]
    [Key("rewards")]
    public List<BuildDeckProductRewardDto> Rewards { get; set; } = new();

    [JsonPropertyName("sales_period_info")]
    [Key("sales_period_info")]
    public List<object> SalesPeriodInfo { get; set; } = new();   // always [] in v1

    [JsonPropertyName("price_crystal")]
    [Key("price_crystal")]
    public int? PriceCrystal { get; set; }

    [JsonPropertyName("price_rupy")]
    [Key("price_rupy")]
    public int? PriceRupy { get; set; }
}

[MessagePackObject]
public class BuildDeckProductRewardDto
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

    [JsonPropertyName("message_id")]
    [Key("message_id")]
    public int MessageId { get; set; }
}

[MessagePackObject]
public class BuildDeckSeriesRewardTierDto
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<BuildDeckProductRewardDto> RewardList { get; set; } = new();

    [JsonPropertyName("is_get")]
    [Key("is_get")]
    public bool IsGet { get; set; }
}
