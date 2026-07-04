using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One assigned mission slot for a viewer. <c>Id</c> is the wire <c>UserMission.id</c> — echoed
/// back as the retire-request parameter, auto-generated. Slot 0 = daily (lot_type=6),
/// Slots 1..3 = weekly (lot_type=2). Progress (<c>total_count</c> on the wire) is NOT stored
/// here — it's read from <see cref="ViewerEventCounter"/> at response-build time, keyed by the
/// catalog row's EventType.
/// </summary>
public class ViewerMission : BaseEntity<long>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override long Id { get; set; }

    public long ViewerId { get; set; }
    public int MissionCatalogId { get; set; }
    public int Slot { get; set; }
    public long AssignedAt { get; set; }
    public long? ClaimedAt { get; set; }
    public int MissionStatus { get; set; } = 1;
}
