using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer, per-pack open counter. Owned collection on <see cref="Viewer"/>.
/// <c>PackId</c> = parent_gacha_id. <c>LastDailyFreeAt</c> is null until the viewer first opens
/// a DAILY (type_detail=3) child gacha; thereafter the controller compares it against the daily
/// reset boundary to decide whether the free open is available again.
/// </summary>
[Owned]
public class ViewerPackOpenCount
{
    public int PackId { get; set; }
    public int OpenCount { get; set; }
    public DateTime? LastDailyFreeAt { get; set; }
}
