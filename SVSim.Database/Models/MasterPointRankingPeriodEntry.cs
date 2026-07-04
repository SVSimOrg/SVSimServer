using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Monthly Master Point ranking window from /mypage/index data.master_point_ranking_period.
/// One row per period; the "current" period is fetched by EndTime > now ordering.
/// </summary>
public class MasterPointRankingPeriodEntry : BaseEntity<int>
{
    public int PeriodNum { get; set; }

    public long NecessaryScore { get; set; }

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
}
