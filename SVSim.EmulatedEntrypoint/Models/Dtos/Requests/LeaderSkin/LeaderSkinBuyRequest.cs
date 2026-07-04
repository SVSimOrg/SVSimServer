using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.LeaderSkin;

/// <summary>
/// /leader_skin/buy request body. sales_type is ShopCommonUtility.SalesType:
/// 0=free, 1=crystal, 2=rupy, 3=ticket (v1: 3 returns 501 — no ticket-priced skin captured).
/// <see cref="ItemId"/> is the ticket item id when paying with a ticket, null otherwise.
/// </summary>
[MessagePackObject]
public class LeaderSkinBuyRequest : BaseRequest
{
    [JsonPropertyName("product_id")]
    [Key("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("sales_type")]
    [Key("sales_type")]
    public int SalesType { get; set; }

    [JsonPropertyName("item_id")]
    [Key("item_id")]
    public long? ItemId { get; set; }
}
