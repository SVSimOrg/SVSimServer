using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One mission template. Id = wire mission_id. Rows are seeded from
/// <c>seeds/mission-catalog.json</c> (extracted from /mission/info captures).
/// LotType 2 = weekly rotation slot; LotType 6 = daily slot (per UserMission.GEM_MISSION_TYPE).
/// EventType is the catalog-side key the progress service matches against; NULL means the row
/// was captured but no event mapping has been added yet (importer logs a warning).
/// </summary>
public class MissionCatalogEntry : BaseEntity<int>
{
    public string Name { get; set; } = "";
    public int LotType { get; set; }
    public int RequireNumber { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
    public int BattlePassPoint { get; set; }
    public bool DefaultFlag { get; set; }
    public string? EventType { get; set; }
    public int? EventArg { get; set; }
    public long StartTime { get; set; }
    public long? EndTime { get; set; }
}
