using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Deck;

public interface IDeckRepository
{
    Task<List<ShadowverseDeckEntry>> GetDecks(long viewerId, Format format);

    /// <summary>
    /// Bulk-fetch viewer decks grouped by format. Returns a dict keyed by every format in
    /// <paramref name="formats"/> — missing formats map to empty lists so callers don't need
    /// dict-existence checks. Single viewer-load, no N+1.
    /// </summary>
    Task<Dictionary<Format, List<ShadowverseDeckEntry>>> GetDecksByFormats(long viewerId, IEnumerable<Format> formats);

    Task<ShadowverseDeckEntry?> GetDeck(long viewerId, Format format, int deckNo);
    Task<int> GetEmptyDeckNumber(long viewerId, Format format);
    Task<ShadowverseDeckEntry> UpsertDeck(long viewerId, Format format, int deckNo, Action<ShadowverseDeckEntry> mutate);
    Task DeleteDecks(long viewerId, Format format, IEnumerable<int> deckNos);
}
