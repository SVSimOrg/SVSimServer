using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One per-buy reward attached to a prebuilt-deck product. Owned by BuildDeckProductEntry.
/// Wire shape: one entry of the product-level `rewards` dict in /build_deck/info, keyed by
/// RewardIndex (the wire string keys "1","2","3").
/// </summary>
[Owned]
public class BuildDeckProductRewardEntry
{
    public int RewardIndex { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
    public int MessageId { get; set; }
}
