using SVSim.Database.Enums;

namespace SVSim.Database.Repositories.BuildDeck;

/// <summary>
/// A story-select deck ready for the wire: presentation metadata from StoryDeckEntry plus the
/// 40-card list expanded from the matching BuildDeckProductEntry. Plain projection, not an entity.
/// </summary>
public sealed class StoryDeckView
{
    public int DeckNo { get; init; }
    public StoryDeckKind Kind { get; init; }
    public int ClassId { get; init; }
    public string DeckName { get; init; } = string.Empty;
    public int SleeveId { get; init; }
    public int LeaderSkinId { get; init; }
    public int IsRecommend { get; init; }
    public int OrderNum { get; init; }
    public int EntryNo { get; init; }
    public int? DeckFormat { get; init; }
    public List<long> CardIdArray { get; init; } = new();
}
