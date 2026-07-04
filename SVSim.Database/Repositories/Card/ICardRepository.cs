using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Card;

public interface ICardRepository
{
    Task<List<ShadowverseCardEntry>> GetAll(bool onlyCollectible);
    Task<List<ShadowverseCardSetEntry>> GetCardSets(bool onlyInRotation);
    Task<List<ShadowverseCardEntry>> GetDefaultCards();
}