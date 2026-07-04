using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.LeaderSkin;

/// <summary>
/// /leader_skin/buy, /leader_skin/buy_set, /leader_skin/buy_set_item all return the same shape:
/// a <c>reward_list</c> of standard <see cref="RewardListEntry"/> entries (post-state totals
/// for currencies, grant counts for cosmetics).
/// </summary>
[MessagePackObject]
public class LeaderSkinBuyResponse
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
