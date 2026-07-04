using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PaymentItemSeed
{
    [JsonPropertyName("record_id")] public int RecordId { get; set; }
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("store_product_id")] public long StoreProductId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("price")] public string Price { get; set; } = "0";
    [JsonPropertyName("charge_crystal_num")] public int ChargeCrystalNum { get; set; }
    [JsonPropertyName("free_crystal_num")] public int FreeCrystalNum { get; set; }
    [JsonPropertyName("purchase_limit")] public int PurchaseLimit { get; set; }
    [JsonPropertyName("special_shop_flag")] public int SpecialShopFlag { get; set; }
    [JsonPropertyName("image_name")] public string ImageName { get; set; } = "";
    [JsonPropertyName("start_time")] public string StartTime { get; set; } = "";
    [JsonPropertyName("end_time")] public string EndTime { get; set; } = "";
    [JsonPropertyName("remaining_time")] public int RemainingTime { get; set; }
    [JsonPropertyName("is_resale_product")] public int IsResaleProduct { get; set; }
    [JsonPropertyName("resale_start_date")] public string ResaleStartDate { get; set; } = "";
}
