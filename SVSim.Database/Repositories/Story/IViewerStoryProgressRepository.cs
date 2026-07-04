using SVSim.Database.Entities.Story;

namespace SVSim.Database.Repositories.Story;

public interface IViewerStoryProgressRepository
{
    Task<Dictionary<int, ViewerStoryProgress>> GetProgressForChaptersAsync(long viewerId, IEnumerable<int> storyIds);
    Task<HashSet<int>> GetBranchUnlockedStoryIdsAsync(long viewerId, IEnumerable<int> storyIds);

    Task UpsertProgressAsync(long viewerId, int storyId, bool? isFinish, bool? isSkipped);
    Task UpsertBranchUnlockAsync(long viewerId, int storyId);
}
