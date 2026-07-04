using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One reward slot belonging to a <see cref="SerialCodeEntry"/>. On redemption each row
/// becomes one <see cref="ViewerPresent"/> in the player's gift inbox.
/// </summary>
public class SerialCodeRewardEntry : BaseEntity<int>
{
    public int SerialCodeId { get; set; }

    /// <summary>0-based ordering within the code's rewards.</summary>
    public int Slot { get; set; }

    /// <summary>UserGoodsType cast to int (matches the wire convention used elsewhere).</summary>
    public int RewardType { get; set; }

    /// <summary>Detail id for the goods. 0 for wallet currencies.</summary>
    public long RewardDetailId { get; set; }

    /// <summary>Positive integer count.</summary>
    public int RewardCount { get; set; }
}
