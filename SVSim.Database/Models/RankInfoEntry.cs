using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class RankInfoEntry : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public int NecessaryPoint { get; set; }
    public int AccumulatePoint { get; set; }
    public int LowerLimitPoint { get; set; }
    public int BaseAddBp { get; set; }
    public int BaseDropBp { get; set; }
    public int StreakBonusPt { get; set; }
    public double WinBonus { get; set; }
    public double LoseBonus { get; set; }
    public int MaxWinBonus { get; set; }
    public int MaxLoseBonus { get; set; }
    public int IsPromotionWar { get; set; }
    public int MatchCount { get; set; }
    public int NecessaryWin { get; set; }
    public int ResetLose { get; set; }
    public int AccumulateMasterPoint { get; set; }
}