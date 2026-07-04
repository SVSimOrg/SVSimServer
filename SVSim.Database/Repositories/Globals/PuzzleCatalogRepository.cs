using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public class PuzzleCatalogRepository : IPuzzleCatalogRepository
{
    private readonly SVSimDbContext _db;
    public PuzzleCatalogRepository(SVSimDbContext db) => _db = db;

    public Task<List<PuzzleGroupEntry>> GetAllGroupsWithPuzzles() =>
        _db.PuzzleGroups
            .Include(g => g.Puzzles)
            .AsNoTracking()
            .AsSplitQuery()   // avoid the cartesian-explode pitfall (CLAUDE.md)
            .OrderBy(g => g.Id)
            .ToListAsync();

    public Task<PuzzleGroupEntry?> GetGroupWithPuzzles(int puzzleMasterId) =>
        _db.PuzzleGroups
            .Include(g => g.Puzzles)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == puzzleMasterId);

    public Task<List<PuzzleMissionEntry>> GetAllMissionsOrdered() =>
        _db.PuzzleMissions
            .AsNoTracking()
            .OrderBy(m => m.OrderId)
            .ThenByDescending(m => m.CampaignCommenceTime)
            .ToListAsync();
}
