using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One row of the BP monthly mission list, keyed to a specific (Year, Month).
/// `RewardType` is nullable because some monthly missions only award BP points (capture shows
/// the "Play 5 Challenge matches" entry has no reward_info block on wire).
/// Id is auto-generated — override BaseEntity's [DatabaseGenerated(None)] default.
/// </summary>
public class BattlePassMonthlyMissionEntry : BaseEntity<int>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override int Id { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public int OrderNum { get; set; }
    public string Name { get; set; } = "";
    public int RequireNumber { get; set; }
    public int BattlePassPoint { get; set; }
    public UserGoodsType? RewardType { get; set; }
    public long? RewardDetailId { get; set; }
    public int? RewardNumber { get; set; }
    public string? EventType { get; set; }
    public int? EventArg { get; set; }
}
