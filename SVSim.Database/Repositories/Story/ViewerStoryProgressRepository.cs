using Microsoft.EntityFrameworkCore;
using SVSim.Database.Entities.Story;

namespace SVSim.Database.Repositories.Story;

public class ViewerStoryProgressRepository : IViewerStoryProgressRepository
{
    private readonly SVSimDbContext _db;
    public ViewerStoryProgressRepository(SVSimDbContext db) { _db = db; }

    public async Task<Dictionary<int, ViewerStoryProgress>> GetProgressForChaptersAsync(
        long viewerId, IEnumerable<int> storyIds)
    {
        var ids = storyIds.ToList();
        var rows = await _db.ViewerStoryProgress
            .Where(p => p.ViewerId == viewerId && ids.Contains(p.StoryId))
            .ToListAsync();
        return rows.ToDictionary(r => r.StoryId);
    }

    public async Task<HashSet<int>> GetBranchUnlockedStoryIdsAsync(long viewerId, IEnumerable<int> storyIds)
    {
        var ids = storyIds.ToList();
        var rows = await _db.ViewerStoryBranchUnlocks
            .Where(u => u.ViewerId == viewerId && ids.Contains(u.StoryId))
            .Select(u => u.StoryId)
            .ToListAsync();
        return new HashSet<int>(rows);
    }

    public async Task UpsertProgressAsync(long viewerId, int storyId, bool? isFinish, bool? isSkipped)
    {
        var row = await _db.ViewerStoryProgress.FirstOrDefaultAsync(
            p => p.ViewerId == viewerId && p.StoryId == storyId);
        if (row is null)
        {
            row = new ViewerStoryProgress { ViewerId = viewerId, StoryId = storyId };
            _db.ViewerStoryProgress.Add(row);
        }
        if (isFinish.HasValue)  { row.IsFinish = isFinish.Value;  if (isFinish.Value)  row.FinishedAt = DateTime.UtcNow; }
        if (isSkipped.HasValue) { row.IsSkipped = isSkipped.Value; if (isSkipped.Value) row.SkippedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
    }

    public async Task UpsertBranchUnlockAsync(long viewerId, int storyId)
    {
        bool exists = await _db.ViewerStoryBranchUnlocks
            .AnyAsync(u => u.ViewerId == viewerId && u.StoryId == storyId);
        if (!exists)
        {
            _db.ViewerStoryBranchUnlocks.Add(new ViewerStoryBranchUnlock
                { ViewerId = viewerId, StoryId = storyId, UnlockedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();
        }
    }
}
