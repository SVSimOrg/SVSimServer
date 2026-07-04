using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// Per-claim record. Presence of a row = is_received: true on the wire reward.
/// Unique on (ViewerId, SeasonId, Track, Level).
/// </summary>
public class ViewerBattlePassClaimEntry : BaseEntity<long>
{
    public long ViewerId { get; set; }
    public int SeasonId { get; set; }
    public BattlePassTrack Track { get; set; }
    public int Level { get; set; }
    public DateTimeOffset ClaimedAt { get; set; }
}
