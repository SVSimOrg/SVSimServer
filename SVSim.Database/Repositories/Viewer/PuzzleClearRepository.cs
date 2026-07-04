using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Viewer;

public class PuzzleClearRepository : IPuzzleClearRepository
{
    private readonly SVSimDbContext _db;
    public PuzzleClearRepository(SVSimDbContext db) => _db = db;

    public async Task<HashSet<int>> GetClearedPuzzleIds(long viewerId)
    {
        var ids = await _db.ViewerPuzzleClears
            .Where(c => c.ViewerId == viewerId)
            .Select(c => c.PuzzleId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    public async Task<Dictionary<int, HashSet<int>>> GetClearedPuzzleIdsByGroup(long viewerId)
    {
        // Join via Puzzles to resolve each cleared PuzzleId to its GroupId.
        var rows = await (
            from c in _db.ViewerPuzzleClears
            where c.ViewerId == viewerId
            join p in _db.Puzzles on c.PuzzleId equals p.Id
            select new { p.GroupId, c.PuzzleId }
        ).ToListAsync();

        return rows
            .GroupBy(r => r.GroupId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.PuzzleId).ToHashSet());
    }

    public async Task UpsertClearAsync(long viewerId, int puzzleId, int retryCount)
    {
        // CONCURRENCY: this read-then-write is not isolated. Two simultaneous /finish calls
        // for the same (viewer, puzzle) could both insert and one will lose to the PK. The
        // wider mission-completion concurrency note lives on PuzzleController.Finish.
        var existing = await _db.ViewerPuzzleClears
            .FirstOrDefaultAsync(c => c.ViewerId == viewerId && c.PuzzleId == puzzleId);

        if (existing is null)
        {
            _db.ViewerPuzzleClears.Add(new ViewerPuzzleClear
            {
                ViewerId = viewerId,
                PuzzleId = puzzleId,
                ClearedAt = DateTime.UtcNow,
                BestRetryCount = retryCount,
            });
        }
        else
        {
            existing.BestRetryCount = Math.Min(existing.BestRetryCount, retryCount);
        }
        await _db.SaveChangesAsync();
    }
}
