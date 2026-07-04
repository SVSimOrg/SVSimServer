using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ItemPurchase;

/// <summary>
/// /item_purchase/purchase request body. <c>rest</c> is the client's locally-cached remaining
/// quota — used as an optional optimistic-concurrency check on the server. Not authoritative;
/// the server's own counter is canonical.
/// </summary>
[MessagePackObject]
public class ItemPurchasePurchaseRequest : BaseRequest
{
    [JsonPropertyName("purchase_id")]
    [Key("purchase_id")]
    public int PurchaseId { get; set; }

    [JsonPropertyName("rest")]
    [Key("rest")]
    public int Rest { get; set; }
}
