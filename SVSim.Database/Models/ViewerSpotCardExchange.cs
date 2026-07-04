namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, exchanged card). Composite PK (ViewerId, CardId). Standalone table
/// (not a Viewer owned collection) to avoid cartesian-explode on viewer-graph reads.
/// <see cref="IsPreRelease"/> snapshot at exchange time so the pre-release counter can be
/// computed without joining back to <see cref="SpotCardExchangeEntry"/> (and to survive
/// catalog edits that re-classify a card).
/// </summary>
public class ViewerSpotCardExchange
{
    public long ViewerId { get; set; }
    public long CardId { get; set; }
    public bool IsPreRelease { get; set; }
    public DateTime ExchangedAt { get; set; }
}
