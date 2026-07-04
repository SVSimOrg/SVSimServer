using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer-per-season progress: gauge total, premium flag, weekly cap bucket.
/// Lazy-created on first /battle_pass/info read. Unique on (ViewerId, SeasonId) per
/// memory project_owned_collection_unique_index.
/// </summary>
public class ViewerBattlePassProgressEntry : BaseEntity<long>
{
    public long ViewerId { get; set; }
    public int SeasonId { get; set; }
    public int CurrentPoint { get; set; }
    public bool IsPremium { get; set; }
    public int WeeklyPoints { get; set; }
    public DateTimeOffset? WeeklyPeriodStart { get; set; }
}
