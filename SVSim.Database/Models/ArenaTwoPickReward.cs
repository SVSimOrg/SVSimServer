// SVSim.Database/Models/ArenaTwoPickReward.cs
using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One row of the Take Two run-end reward table. Multiple rows per <see cref="WinCount"/>
/// (e.g. 1 ticket + N rupies = 2 rows). Seeded by <c>ArenaTwoPickRewardImporter</c> from
/// <c>SVSim.Bootstrap/Data/seeds/arena-two-pick-rewards.json</c>.
/// </summary>
[Index(nameof(WinCount))]
[Index(nameof(WinCount), nameof(RewardGroup), nameof(RewardType), nameof(RewardId), nameof(RewardNum), IsUnique = true)]
public class ArenaTwoPickReward
{
    public long Id { get; set; }

    /// <summary>0..MaxWins. Run ends at LossCount==2 or WinCount==MAX(WinCount).</summary>
    public int WinCount { get; set; }

    /// <summary>
    /// Groups rows into independent pick buckets. At finish/retire time one row is
    /// weighted-picked per group. Default 0 keeps legacy rows in a single group.
    /// </summary>
    public int RewardGroup { get; set; }

    /// <summary>
    /// Relative probability weight for this row within its <see cref="RewardGroup"/>.
    /// Weight == 0 rows are excluded from picking. Default 1.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary><see cref="UserGoodsType"/> on the wire (e.g. Item=4, Rupy=9).</summary>
    public UserGoodsType RewardType { get; set; }

    /// <summary>Item id for Item; 0 for currencies.</summary>
    public long RewardId { get; set; }

    /// <summary>Count (e.g. ticket quantity or rupy amount). 0 = "no reward" outcome.</summary>
    public int RewardNum { get; set; }
}
