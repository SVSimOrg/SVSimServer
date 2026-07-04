using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Collectibles;

public class CollectionRepository : ICollectionRepository
{
    private readonly SVSimDbContext _dbContext;

    public CollectionRepository(SVSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LeaderSkinEntry>> GetLeaderSkins()
    {
        return await _dbContext.Set<LeaderSkinEntry>().AsNoTracking().Include(skin => skin.Class).ToListAsync();
    }

    public Task<List<int>> GetAllSleeveIds() =>
        _dbContext.Set<SleeveEntry>().AsNoTracking().Select(s => s.Id).ToListAsync();

    public Task<List<int>> GetAllEmblemIds() =>
        _dbContext.Set<EmblemEntry>().AsNoTracking().Select(e => e.Id).ToListAsync();

    public Task<List<int>> GetAllDegreeIds() =>
        _dbContext.Set<DegreeEntry>().AsNoTracking().Select(d => d.Id).ToListAsync();

    public Task<List<int>> GetAllMyPageBackgroundIds() =>
        _dbContext.Set<MyPageBackgroundEntry>().AsNoTracking().Select(m => m.Id).ToListAsync();
}