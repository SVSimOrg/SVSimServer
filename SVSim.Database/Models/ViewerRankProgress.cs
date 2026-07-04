using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, format) tracking accumulated rank points and master points.
/// Point is 0-based cumulative; the current rank_id is derived at read time from Point
/// (and MasterPoint once past 50000). Owned collection on <see cref="Viewer"/>.
/// </summary>
[Owned]
public class ViewerRankProgress
{
    public Format Format { get; set; }
    public int Point { get; set; }
    public int MasterPoint { get; set; }
}
