using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One per-buy reward attached to a sleeve product. Owned by <see cref="SleeveShopProductEntry"/>.
/// Wire shape: one entry of the product-level `rewards` array in /sleeve/info. Order is
/// preserved by <see cref="OrderIndex"/> since the wire shape is an ordered array, not a dict.
/// </summary>
[Owned]
public class SleeveShopProductRewardEntry
{
    public int OrderIndex { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
}
