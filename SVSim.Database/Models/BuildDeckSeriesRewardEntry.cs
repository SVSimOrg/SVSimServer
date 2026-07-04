using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One tier-reward item attached to a prebuilt-deck series. Owned by BuildDeckSeriesEntry.
/// Wire shape: flattened from /build_deck/info's `series_rewards` dict — each tier (keyed
/// by total-purchases-from-series threshold) carries a list of rewards; this row is one
/// (TierIndex, ItemIndex) cell.
/// </summary>
[Owned]
public class BuildDeckSeriesRewardEntry
{
    public int TierIndex { get; set; }   // 1, 2, 3, ... — unlock threshold
    public int ItemIndex { get; set; }   // ordinal within tier
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
    public int MessageId { get; set; }
}
