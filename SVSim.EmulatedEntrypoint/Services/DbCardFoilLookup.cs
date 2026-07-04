using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.EmulatedEntrypoint.Services;

public class DbCardFoilLookup : ICardFoilLookup
{
    private readonly SVSimDbContext _db;
    public DbCardFoilLookup(SVSimDbContext db) { _db = db; }

    public ShadowverseCardEntry? TryGetFoilTwin(long baseCardId) =>
        _db.Cards.FirstOrDefault(c => c.Id == baseCardId + 1 && c.IsFoil);
}
