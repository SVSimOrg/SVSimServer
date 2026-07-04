using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class ItemPurchaseSeed
{
    [JsonPropertyName("purchase_id")] public int PurchaseId { get; set; }
    [JsonPropertyName("require_item_type")] public int RequireItemType { get; set; }
    [JsonPropertyName("require_item_id")] public long RequireItemId { get; set; }
    [JsonPropertyName("require_item_num")] public int RequireItemNum { get; set; }
    [JsonPropertyName("purchase_item_type")] public int PurchaseItemType { get; set; }
    [JsonPropertyName("purchase_item_id")] public long PurchaseItemId { get; set; }
    [JsonPropertyName("purchase_item_num")] public int PurchaseItemNum { get; set; }
    [JsonPropertyName("purchase_name")] public string PurchaseName { get; set; } = "";
    [JsonPropertyName("is_monthly_reset")] public bool IsMonthlyReset { get; set; }
    [JsonPropertyName("purchase_limit")] public int PurchaseLimit { get; set; }
}
