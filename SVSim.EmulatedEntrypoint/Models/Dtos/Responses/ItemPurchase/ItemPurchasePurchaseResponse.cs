using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ItemPurchase;

/// <summary>
/// /item_purchase/purchase response. <c>reward_list</c> uses the standard
/// <see cref="RewardListEntry"/> shape: post-state totals for currencies, grant counts for
/// items/cards. First entry is the debit-side post-state for the require_item; subsequent
/// entries are the grant(s) from RewardGrantService.
/// </summary>
[MessagePackObject]
public class ItemPurchasePurchaseResponse
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
