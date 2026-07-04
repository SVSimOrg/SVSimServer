using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One tier of an achievement. PK is composite (AchievementType, Level). Rows are seeded from
/// <c>seeds/achievement-catalog.json</c>. The captured tier IS the max tier in our world —
/// max_level on the wire is computed as MAX(Level) per AchievementType at /mission/info time.
/// Inherits Id from BaseEntity but the Id is unused; PK is configured in DbContext.
/// </summary>
public class AchievementCatalogEntry
{
    public int AchievementType { get; set; }
    public int Level { get; set; }
    public string Name { get; set; } = "";
    public int RequireNumber { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
    public int OrderNum { get; set; }
    public string? EventType { get; set; }
    public int? EventArg { get; set; }
}
