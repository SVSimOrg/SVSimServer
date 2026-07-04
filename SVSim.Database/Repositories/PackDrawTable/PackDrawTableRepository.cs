using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Repositories.PackDrawTables;

public class PackDrawTableRepository : IPackDrawTableRepository
{
    private readonly SVSimDbContext _db;
    public PackDrawTableRepository(SVSimDbContext db) { _db = db; }

    public async Task<PackDrawTable?> GetAsync(int packId)
    {
        var config = await _db.PackDrawConfigs.FirstOrDefaultAsync(c => c.Id == packId);
        if (config is null) return null;

        var slotRates = await _db.PackDrawSlotRates
            .Where(s => s.PackId == packId)
            .ToListAsync();

        var cardWeights = await _db.PackDrawCardWeights
            .Where(w => w.PackId == packId)
            .ToListAsync();

        return new PackDrawTable
        {
            Config = config,
            SlotRates = slotRates,
            CardWeights = cardWeights,
        };
    }
}
