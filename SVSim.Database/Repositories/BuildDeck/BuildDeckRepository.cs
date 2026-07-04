using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BuildDeck;

public class BuildDeckRepository : IBuildDeckRepository
{
    private readonly SVSimDbContext _db;
    public BuildDeckRepository(SVSimDbContext db) { _db = db; }

    public async Task<List<BuildDeckSeriesEntry>> GetEnabledCatalog(int addSeriesId)
    {
        var q = _db.BuildDeckSeries
            .Include(s => s.SeriesRewards)
            .Include(s => s.Products.Where(p => p.IsEnabled))
                .ThenInclude(p => p.Cards)
            .Include(s => s.Products.Where(p => p.IsEnabled))
                .ThenInclude(p => p.Rewards)
            .Where(s => s.IsEnabled)
            .AsSplitQuery();

        if (addSeriesId != 0)
        {
            q = q.Where(s => s.Id == addSeriesId);
        }

        var list = await q.ToListAsync();
        list.Sort((a, b) => b.OrderIndex.CompareTo(a.OrderIndex));
        return list;
    }

    public async Task<BuildDeckProductEntry?> GetProduct(int productId) =>
        await _db.BuildDeckProducts
            .Include(p => p.Cards)
            .Include(p => p.Rewards)
            .Include(p => p.Series).ThenInclude(s => s!.SeriesRewards)
            .Include(p => p.Series).ThenInclude(s => s!.Products)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<Dictionary<int, ViewerBuildDeckProductPurchase>> GetPurchasesForViewer(long viewerId)
    {
        var viewer = await _db.Viewers
            .Include(v => v.BuildDeckPurchases)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        return viewer?.BuildDeckPurchases.ToDictionary(p => p.ProductId) ?? new();
    }

    public async Task<int> IncrementPurchaseCount(long viewerId, int productId)
    {
        var viewer = await _db.Viewers
            .Include(v => v.BuildDeckPurchases)
            .FirstAsync(v => v.Id == viewerId);
        var row = viewer.BuildDeckPurchases.FirstOrDefault(p => p.ProductId == productId);
        if (row is null)
        {
            row = new ViewerBuildDeckProductPurchase { ProductId = productId, PurchaseCount = 1 };
            viewer.BuildDeckPurchases.Add(row);
        }
        else
        {
            row.PurchaseCount += 1;
        }
        await _db.SaveChangesAsync();
        return row.PurchaseCount;
    }

    public async Task<List<StoryDeckView>> GetStoryDecksByClass(int classId)
    {
        var decks = await _db.StoryDecks.Where(d => d.ClassId == classId).ToListAsync();
        if (decks.Count == 0) return new();

        var ids = decks.Select(d => d.DeckNo).ToList();
        var products = await _db.BuildDeckProducts
            .Where(p => ids.Contains(p.Id))
            .Include(p => p.Cards)
            .AsSplitQuery()
            .ToListAsync();

        // Expand each product's owned card rows by Number into a flat card_id list (spots included —
        // validated against the prod capture, 112/112 match).
        var cardsById = products.ToDictionary(
            p => p.Id,
            p => p.Cards.SelectMany(c => Enumerable.Repeat(c.CardId, c.Number)).ToList());

        return decks.Select(d => new StoryDeckView
        {
            DeckNo = d.DeckNo,
            Kind = d.Kind,
            ClassId = d.ClassId,
            DeckName = d.DeckName,
            SleeveId = d.SleeveId,
            LeaderSkinId = d.LeaderSkinId,
            IsRecommend = d.IsRecommend,
            OrderNum = d.OrderNum,
            EntryNo = d.EntryNo,
            DeckFormat = d.DeckFormat,
            CardIdArray = cardsById.TryGetValue(d.DeckNo, out var cards) ? cards : new(),
        }).ToList();
    }
}
