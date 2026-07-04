using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One battle pass season. Active season is resolved by time-window
/// (StartDate &lt;= now &lt; EndDate). Rewards are loaded via Rewards collection.
/// </summary>
public class BattlePassSeasonEntry : BaseEntity<int>
{
    public string Name { get; set; } = "";
    public int MaxLevel { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool CanPurchase { get; set; }
    public int PriceCrystal { get; set; }
    public string Description { get; set; } = "";

    public List<BattlePassRewardEntry> Rewards { get; set; } = new();
}
