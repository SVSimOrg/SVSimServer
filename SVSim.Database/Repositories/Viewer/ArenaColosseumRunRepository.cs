using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Viewer;

public class ArenaColosseumRunRepository : IArenaColosseumRunRepository
{
    private readonly SVSimDbContext _db;
    public ArenaColosseumRunRepository(SVSimDbContext db) => _db = db;

    public Task<ViewerArenaColosseumRun?> GetByViewerIdAsync(long viewerId) =>
        _db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == viewerId);

    public async Task UpsertAsync(ViewerArenaColosseumRun run)
    {
        run.UpdatedAt = DateTime.UtcNow;
        if (run.Id == 0)
        {
            run.CreatedAt = DateTime.UtcNow;
            _db.ViewerArenaColosseumRuns.Add(run);
        }
        else
        {
            _db.ViewerArenaColosseumRuns.Update(run);
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long viewerId)
    {
        var row = await _db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == viewerId);
        if (row is null) return;
        _db.ViewerArenaColosseumRuns.Remove(row);
        await _db.SaveChangesAsync();
    }
}
