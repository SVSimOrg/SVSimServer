using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Viewer;

public class ArenaTwoPickRunRepository : IArenaTwoPickRunRepository
{
    private readonly SVSimDbContext _db;
    public ArenaTwoPickRunRepository(SVSimDbContext db) => _db = db;

    public Task<ViewerArenaTwoPickRun?> GetByViewerIdAsync(long viewerId) =>
        _db.ViewerArenaTwoPickRuns.FirstOrDefaultAsync(r => r.ViewerId == viewerId);

    public async Task UpsertAsync(ViewerArenaTwoPickRun run)
    {
        run.UpdatedAt = DateTime.UtcNow;
        if (run.Id == 0)
        {
            run.CreatedAt = DateTime.UtcNow;
            _db.ViewerArenaTwoPickRuns.Add(run);
        }
        else
        {
            _db.ViewerArenaTwoPickRuns.Update(run);
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long viewerId)
    {
        var row = await _db.ViewerArenaTwoPickRuns.FirstOrDefaultAsync(r => r.ViewerId == viewerId);
        if (row is null) return;
        _db.ViewerArenaTwoPickRuns.Remove(row);
        await _db.SaveChangesAsync();
    }
}
