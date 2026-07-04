using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Card;

public class CardRepository : BaseRepository<ShadowverseCardEntry>, ICardRepository
{
    public CardRepository(SVSimDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<List<ShadowverseCardEntry>> GetAll(bool onlyCollectible)
    {
        var cards = await DbSet.AsNoTracking().Where(card => !onlyCollectible || card.CollectionInfo != null).ToListAsync();
        return cards;
    }

    public async Task<List<ShadowverseCardEntry>> GetDefaultCards()
    {
        // The set of cards every viewer is treated as owning 3-of from the start: bronze,
        // non-foil basics. Silver/gold basics and animated (foil) variants are earned
        // through story rewards etc.
        return await DbContext.Set<ShadowverseCardSetEntry>().Where(set => set.IsBasic)
            .SelectMany(set => set.Cards)
            .Where(card => card.Rarity == Enums.Rarity.Bronze && !card.IsFoil)
            .ToListAsync();
    }

    public async Task<List<ShadowverseCardSetEntry>> GetCardSets(bool onlyInRotation)
    {
        return await DbContext.Set<ShadowverseCardSetEntry>().AsNoTracking()
            .Where(set => !onlyInRotation || set.IsInRotation).ToListAsync();
    }
}