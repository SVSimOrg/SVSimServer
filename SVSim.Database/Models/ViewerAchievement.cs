namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer state for one achievement type. Composite PK (ViewerId, AchievementType) configured
/// in DbContext. <c>Level</c> is the viewer's current tier; <c>max_level</c> on the wire is
/// derived from catalog as MAX(Level) per type. Lazy-created at /load/index time — one row per
/// AchievementCatalogEntries.AchievementType that the viewer doesn't yet have a row for.
/// </summary>
public class ViewerAchievement
{
    public long ViewerId { get; set; }
    public int AchievementType { get; set; }
    public int Level { get; set; } = 1;
    public int AchievementStatus { get; set; }
    public int NowAchievedLevel { get; set; }
    public int ResultAnnounceSawLevel { get; set; }
}
