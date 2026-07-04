using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Deck;

public class DeckRepository : IDeckRepository
{
    private const int MaxDecksPerFormat = 50;

    private readonly SVSimDbContext _dbContext;

    public DeckRepository(SVSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ShadowverseDeckEntry>> GetDecks(long viewerId, Format format)
    {
        var viewer = await _dbContext.Viewers
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Decks).ThenInclude(d => d.Class)
            .Include(v => v.Decks).ThenInclude(d => d.Sleeve)
            .Include(v => v.Decks).ThenInclude(d => d.LeaderSkin)
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(v => v.Id == viewerId);

        return viewer?.Decks.Where(d => d.Format == format).OrderBy(d => d.Number).ToList()
               ?? new List<ShadowverseDeckEntry>();
    }

    public async Task<Dictionary<Format, List<ShadowverseDeckEntry>>> GetDecksByFormats(long viewerId, IEnumerable<Format> formats)
    {
        var requested = formats.ToHashSet();
        var viewer = await _dbContext.Viewers
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Decks).ThenInclude(d => d.Class)
            .Include(v => v.Decks).ThenInclude(d => d.Sleeve)
            .Include(v => v.Decks).ThenInclude(d => d.LeaderSkin)
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(v => v.Id == viewerId);

        // Seed every requested format with an empty list so callers iterate without null checks.
        var result = requested.ToDictionary(f => f, _ => new List<ShadowverseDeckEntry>());
        if (viewer is null) return result;

        foreach (var deck in viewer.Decks.Where(d => requested.Contains(d.Format)).OrderBy(d => d.Number))
        {
            result[deck.Format].Add(deck);
        }
        return result;
    }

    public async Task<ShadowverseDeckEntry?> GetDeck(long viewerId, Format format, int deckNo)
    {
        var viewer = await _dbContext.Viewers
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Decks).ThenInclude(d => d.Class)
            .Include(v => v.Decks).ThenInclude(d => d.Sleeve)
            .Include(v => v.Decks).ThenInclude(d => d.LeaderSkin)
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(v => v.Id == viewerId);

        return viewer?.Decks.FirstOrDefault(d => d.Format == format && d.Number == deckNo);
    }

    public async Task<int> GetEmptyDeckNumber(long viewerId, Format format)
    {
        var taken = (await GetDecks(viewerId, format)).Select(d => d.Number).ToHashSet();
        for (int i = 1; i <= MaxDecksPerFormat; i++)
        {
            if (!taken.Contains(i)) return i;
        }
        return 0;
    }

    public async Task<ShadowverseDeckEntry> UpsertDeck(long viewerId, Format format, int deckNo,
        Action<ShadowverseDeckEntry> mutate)
    {
        var viewer = await _dbContext.Viewers
            .AsSplitQuery()
            .Include(v => v.Decks).ThenInclude(d => d.Class)
            .Include(v => v.Decks).ThenInclude(d => d.Sleeve)
            .Include(v => v.Decks).ThenInclude(d => d.LeaderSkin)
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(v => v.Id == viewerId)
            ?? throw new InvalidOperationException($"Viewer {viewerId} not found.");

        var deck = viewer.Decks.FirstOrDefault(d => d.Format == format && d.Number == deckNo);
        if (deck is null)
        {
            deck = new ShadowverseDeckEntry { Format = format, Number = deckNo };
            viewer.Decks.Add(deck);
        }

        mutate(deck);
        await _dbContext.SaveChangesAsync();
        return deck;
    }

    public async Task DeleteDecks(long viewerId, Format format, IEnumerable<int> deckNos)
    {
        var viewer = await _dbContext.Viewers
            .Include(v => v.Decks)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        if (viewer is null) return;

        var nos = deckNos.ToHashSet();
        var toRemove = viewer.Decks.Where(d => d.Format == format && nos.Contains(d.Number)).ToList();
        // Decks.ViewerId is nullable, so removing from the collection alone just orphans the
        // row (clears ViewerId, leaves the deck in the DB). Delete from the DbSet directly so
        // is_delete=1 and /deck/delete_deck_list actually remove the row.
        _dbContext.Decks.RemoveRange(toRemove);
        await _dbContext.SaveChangesAsync();
    }
}
