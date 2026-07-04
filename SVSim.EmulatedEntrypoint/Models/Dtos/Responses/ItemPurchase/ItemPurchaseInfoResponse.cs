using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ItemPurchase;

/// <summary>
/// /item_purchase/info response.
/// <para>
/// <c>item_purchase_info</c> is an array of catalog entries with per-viewer <c>rest</c>
/// (PurchaseLimit minus the viewer's counter for the relevant period).
/// </para>
/// <para>
/// <c>user_card_pack_ticket_list</c> is the FULL set of card-pack-ticket items (catalog
/// Items.Type == 2) joined with the viewer's owned counts — even zero counts are emitted, as
/// the client's parser unconditionally calls <c>PlayerStaticData.UpdateItemNum(item_id, number)</c>
/// for every entry to refresh its in-memory mapping.
/// </para>
/// </summary>
[MessagePackObject]
public class ItemPurchaseInfoResponse
{
    [JsonPropertyName("item_purchase_info")]
    [Key("item_purchase_info")]
    public List<ItemPurchaseEntryDto> ItemPurchaseInfo { get; set; } = new();

    [JsonPropertyName("user_card_pack_ticket_list")]
    [Key("user_card_pack_ticket_list")]
    public List<UserCardPackTicketDto> UserCardPackTicketList { get; set; } = new();
}

[MessagePackObject]
public class ItemPurchaseEntryDto
{
    [JsonPropertyName("purchase_id")]
    [Key("purchase_id")]
    public int PurchaseId { get; set; }

    [JsonPropertyName("require_item_type")]
    [Key("require_item_type")]
    public int RequireItemType { get; set; }

    [JsonPropertyName("require_item_id")]
    [Key("require_item_id")]
    public long RequireItemId { get; set; }

    [JsonPropertyName("require_item_num")]
    [Key("require_item_num")]
    public int RequireItemNum { get; set; }

    [JsonPropertyName("purchase_name")]
    [Key("purchase_name")]
    public string PurchaseName { get; set; } = string.Empty;

    [JsonPropertyName("purchase_item_type")]
    [Key("purchase_item_type")]
    public int PurchaseItemType { get; set; }

    [JsonPropertyName("purchase_item_id")]
    [Key("purchase_item_id")]
    public long PurchaseItemId { get; set; }

    [JsonPropertyName("purchase_item_num")]
    [Key("purchase_item_num")]
    public int PurchaseItemNum { get; set; }

    /// <summary>0 or 1 — client compares to int 0.</summary>
    [JsonPropertyName("is_monthly_reset")]
    [Key("is_monthly_reset")]
    public int IsMonthlyReset { get; set; }

    [JsonPropertyName("rest")]
    [Key("rest")]
    public int Rest { get; set; }
}

[MessagePackObject]
public class UserCardPackTicketDto
{
    [JsonPropertyName("item_id")]
    [Key("item_id")]
    public int ItemId { get; set; }

    [JsonPropertyName("number")]
    [Key("number")]
    public int Number { get; set; }
}
