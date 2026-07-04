using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Projects inventory <see cref="GrantedReward"/> results (post-state totals or per-grant deltas)
/// into the wire <see cref="RewardListEntry"/> shape. The <c>reward_type</c> enum is widened to its
/// int wire value at this single boundary. Replaces the per-endpoint copies of this projection
/// (pack/open, leader_skin/buy*, build_deck/buy, sleeve/buy, item_purchase, spot_card_exchange,
/// gacha-point exchange).
/// </summary>
public static class RewardListExtensions
{
    public static List<RewardListEntry> ToRewardList(this IEnumerable<GrantedReward> grants) =>
        grants.Select(g => new RewardListEntry
        {
            RewardType = (int)g.RewardType,
            RewardId = g.RewardId,
            RewardNum = g.RewardNum,
        }).ToList();
}
