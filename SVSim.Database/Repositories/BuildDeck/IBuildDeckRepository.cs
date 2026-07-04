using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BuildDeck;

public interface IBuildDeckRepository
{
    /// <summary>
    /// Load enabled series (filtered by addSeriesId when non-zero) with all owned children
    /// for /build_deck/info. Series and per-series products are sorted by OrderIndex desc.
    /// </summary>
    Task<List<BuildDeckSeriesEntry>> GetEnabledCatalog(int addSeriesId);

    /// <summary>
    /// Load a single product (with Series + Cards + Rewards + Series.SeriesRewards) by id.
    /// Returns null if absent. Used by /build_deck/buy and /build_deck/get_purchase_count.
    /// </summary>
    Task<BuildDeckProductEntry?> GetProduct(int productId);

    /// <summary>
    /// Per-viewer purchase counter snapshot. Key = product_id.
    /// </summary>
    Task<Dictionary<int, ViewerBuildDeckProductPurchase>> GetPurchasesForViewer(long viewerId);

    /// <summary>
    /// Increment the (ViewerId, ProductId) purchase counter by 1 (insert if absent).
    /// Returns the new total.
    /// </summary>
    Task<int> IncrementPurchaseCount(long viewerId, int productId);

    /// <summary>
    /// Story deck-select decks for a class: StoryDeckEntry presentation rows joined to the matching
    /// BuildDeckProductEntry card lists (deck_no == product_id), expanded to a flat card_id array.
    /// Returns build and trial decks together; the caller splits by Kind.
    /// </summary>
    Task<List<StoryDeckView>> GetStoryDecksByClass(int classId);
}
