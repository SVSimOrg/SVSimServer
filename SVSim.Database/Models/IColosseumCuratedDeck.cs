namespace SVSim.Database.Models;

/// <summary>
/// Common shape across the three curated-deck pool tables — used by the controller's
/// generic list+register dispatcher (Phase 3 Task 10) to avoid duplicating identical code
/// per pool. The interface is plumbing only; the pools stay distinct EF entity types.
/// </summary>
public interface IColosseumCuratedDeck
{
    long Id { get; set; }
    int DeckNo { get; set; }
    int ClassId { get; set; }
    string CardListJson { get; set; }
    long SleeveId { get; set; }
    long LeaderSkinId { get; set; }
    int DisplayOrder { get; set; }
}
