using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Pack;

public class PackRepository : IPackRepository
{
    private readonly SVSimDbContext _db;
    public PackRepository(SVSimDbContext db) { _db = db; }

    public async Task<List<PackConfigEntry>> GetActivePacks(DateTime now) =>
        await _db.Packs
            .AsSplitQuery()
            .Include(p => p.ChildGachas)
            .Include(p => p.Banners)
            .Where(p => p.IsEnabled && p.CommenceDate <= now && p.CompleteDate >= now)
            // parent_gacha_id DESC matches the prod /pack/info wire order. The tutorial pack
            // UI runs with controls locked and auto-selects the FIRST entry in
            // pack_config_list, so the legendary starter pack (99047) MUST be index 0 for the
            // tutorial to progress. Verified against data_dumps/captures/traffic_prod_tutorial.ndjson —
            // prod emits [99047, 92001, 80047, 16015..16011, 10032..10001].
            .OrderByDescending(p => p.Id)
            .ToListAsync();

    public async Task<PackConfigEntry?> GetPack(int parentGachaId) =>
        await _db.Packs
            .AsSplitQuery()
            .Include(p => p.ChildGachas)
            .Include(p => p.Banners)
            .FirstOrDefaultAsync(p => p.Id == parentGachaId);

    public async Task<Dictionary<int, ViewerPackOpenCount>> GetOpenCountsForViewer(long viewerId)
    {
        var viewer = await _db.Viewers
            .Include(v => v.PackOpenCounts)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        return viewer?.PackOpenCounts.ToDictionary(p => p.PackId) ?? new();
    }

    public async Task IncrementOpenCount(long viewerId, int parentGachaId, int by)
    {
        var viewer = await _db.Viewers
            .Include(v => v.PackOpenCounts)
            .FirstAsync(v => v.Id == viewerId);
        var row = viewer.PackOpenCounts.FirstOrDefault(p => p.PackId == parentGachaId);
        if (row is null)
        {
            viewer.PackOpenCounts.Add(new ViewerPackOpenCount { PackId = parentGachaId, OpenCount = by });
        }
        else
        {
            row.OpenCount += by;
        }
        await _db.SaveChangesAsync();
    }

    public async Task MarkDailyFreeUsed(long viewerId, int parentGachaId, DateTime when)
    {
        var viewer = await _db.Viewers
            .Include(v => v.PackOpenCounts)
            .FirstAsync(v => v.Id == viewerId);
        var row = viewer.PackOpenCounts.FirstOrDefault(p => p.PackId == parentGachaId);
        if (row is null)
        {
            viewer.PackOpenCounts.Add(new ViewerPackOpenCount { PackId = parentGachaId, OpenCount = 0, LastDailyFreeAt = when });
        }
        else
        {
            row.LastDailyFreeAt = when;
        }
        await _db.SaveChangesAsync();
    }

}
