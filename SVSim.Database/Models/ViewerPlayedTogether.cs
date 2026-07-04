namespace SVSim.Database.Models;

/// <summary>
/// One row per (owner, opponent) pair. Upserted on each new battle so the table
/// holds at most one row per opponent. Per-viewer 50-row retention cap pruned
/// by <c>IPlayedTogetherWriter.RecordAsync</c>.
/// </summary>
public class ViewerPlayedTogether
{
    public long OwnerViewerId { get; set; }
    public long OpponentViewerId { get; set; }
    public DateTime PlayedAt { get; set; }
    public int PlayedMode { get; set; }
    public int BattleType { get; set; }
    public int DeckFormat { get; set; }
    public int TwoPickType { get; set; }
}
