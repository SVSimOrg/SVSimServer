using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One per-buy reward attached to a leader-skin product. Owned by
/// <see cref="LeaderSkinShopProductEntry"/>. Captures show each skin product bundles 3 rewards:
/// the skin itself (type=10), the matching emblem (type=7), and the matching sleeve (type=6).
/// </summary>
[Owned]
public class LeaderSkinShopProductRewardEntry
{
    public int OrderIndex { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
}
