using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One reward cell on a battle pass season (track × level). Capture shows at most one
/// reward per (season, track, level) — enforced by unique index in DbContext.
/// RewardType integers come from <see cref="UserGoodsType"/>.
/// </summary>
public class BattlePassRewardEntry : BaseEntity<long>
{
    public int SeasonId { get; set; }
    public BattlePassTrack Track { get; set; }
    public int Level { get; set; }
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
    public bool IsAppealExclusion { get; set; }

    public BattlePassSeasonEntry Season { get; set; } = null!;
}
