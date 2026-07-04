namespace SVSim.Database.Models;

/// <summary>
/// One of the 2 pick-sets offered to the player on the current draft turn. Persisted as
/// part of <see cref="ViewerArenaTwoPickRun.PendingPickSetsJson"/>. <see cref="Id"/> is the
/// monotonic counter the client sends back as <c>selected_id</c> on /card_choose.
/// </summary>
public class CandidatePair
{
    public long Id { get; set; }
    public int Turn { get; set; }
    public int SetNum { get; set; }
    public long CardId1 { get; set; }
    public long CardId2 { get; set; }
    public bool IsSelected { get; set; }
}
