namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, leader-skin series) marking that the viewer has claimed the
/// series-completion bonus via /leader_skin/buy_set_item. Composite PK (ViewerId, SeriesId).
/// Standalone table (not a Viewer owned collection) to avoid the cartesian-explode pitfall
/// when loading the viewer graph — claim state is checked per-series, not per-viewer-load.
/// </summary>
public class ViewerLeaderSkinSetClaim
{
    public long ViewerId { get; set; }
    public int SeriesId { get; set; }
    public DateTime ClaimedAt { get; set; }
}
