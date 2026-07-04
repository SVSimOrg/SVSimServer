using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry under /payment_pc/item_list data, parsed by PaymentItemListTask
/// (Cute/PaymentItemListTask.cs:43-70). The client iterates via int index and reads
/// 8 fields unconditionally (store_product_id, name, text, purchase_limit, id, image_name,
/// end_time, special_shop_flag), with number_of_product_purchased TryGetValue-guarded.
///
/// All wire fields are PHP-stringified EXCEPT <c>purchase_num_current</c>, which is a true int.
/// String-typed properties avoid JsonConverter machinery — the controller stringifies typed DB
/// columns via ToString(InvariantCulture) on the way out, same approach as MyPageController.BuildBannerInfo.
///
/// Prod-captured shape (one entry):
/// <code>
/// {"record_id":"21","id":"8","store_product_id":"10011",
///  "name":"60-crystal set","text":"Purchase 60 Crystals","price":"0.99",
///  "charge_crystal_num":"60","free_crystal_num":"0","purchase_limit":"999999999",
///  "special_shop_flag":"0","image_name":"thumbnail_crystal",
///  "start_time":"2022-10-05 15:00:00","end_time":"2030-03-01 14:59:59",
///  "remaining_time":"0","is_resale_product":"0","resale_start_date":"","purchase_num_current":0}
/// </code>
/// </summary>
[MessagePackObject]
public class PaymentItemInfo
{
    [JsonPropertyName("record_id")]
    [Key("record_id")]
    public string RecordId { get; set; } = "0";

    [JsonPropertyName("id")]
    [Key("id")]
    public string Id { get; set; } = "0";

    [JsonPropertyName("store_product_id")]
    [Key("store_product_id")]
    public string StoreProductId { get; set; } = "0";

    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    [Key("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Decimal as PHP-stringified value (e.g. "0.99"). Preserves prod's wire convention.</summary>
    [JsonPropertyName("price")]
    [Key("price")]
    public string Price { get; set; } = "0";

    [JsonPropertyName("charge_crystal_num")]
    [Key("charge_crystal_num")]
    public string ChargeCrystalNum { get; set; } = "0";

    [JsonPropertyName("free_crystal_num")]
    [Key("free_crystal_num")]
    public string FreeCrystalNum { get; set; } = "0";

    [JsonPropertyName("purchase_limit")]
    [Key("purchase_limit")]
    public string PurchaseLimit { get; set; } = "0";

    [JsonPropertyName("special_shop_flag")]
    [Key("special_shop_flag")]
    public string SpecialShopFlag { get; set; } = "0";

    [JsonPropertyName("image_name")]
    [Key("image_name")]
    public string ImageName { get; set; } = string.Empty;

    /// <summary>"yyyy-MM-dd HH:mm:ss" wire format.</summary>
    [JsonPropertyName("start_time")]
    [Key("start_time")]
    public string StartTime { get; set; } = string.Empty;

    /// <summary>"yyyy-MM-dd HH:mm:ss" wire format.</summary>
    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("remaining_time")]
    [Key("remaining_time")]
    public string RemainingTime { get; set; } = "0";

    [JsonPropertyName("is_resale_product")]
    [Key("is_resale_product")]
    public string IsResaleProduct { get; set; } = "0";

    /// <summary>Empty string ("") when unset; otherwise "yyyy-MM-dd HH:mm:ss". Matches prod.</summary>
    [JsonPropertyName("resale_start_date")]
    [Key("resale_start_date")]
    public string ResaleStartDate { get; set; } = string.Empty;

    /// <summary>True int on the wire (not string) — count of this viewer's purchases of this product.
    /// Per-viewer state; currently hardcoded to 0 server-side until purchase tracking lands.</summary>
    [JsonPropertyName("purchase_num_current")]
    [Key("purchase_num_current")]
    public int PurchaseNumCurrent { get; set; }
}
