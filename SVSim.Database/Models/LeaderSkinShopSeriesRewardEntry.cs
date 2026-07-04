using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One set-completion bonus item attached to a leader-skin series. Owned by
/// <see cref="LeaderSkinShopSeriesEntry"/>. Granted by /leader_skin/buy_set_item once the
/// viewer owns every skin in the series. Wire shape: entries inside
/// <c>rewards.items[]</c> on the per-series block of /leader_skin/products.
/// </summary>
[Owned]
public class LeaderSkinShopSeriesRewardEntry
{
    public int OrderIndex { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
}
