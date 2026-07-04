namespace SVSim.Database.Repositories.Viewer;

public interface IPuzzleClearRepository
{
    /// <summary>Returns the set of puzzle_ids this viewer has cleared.</summary>
    Task<HashSet<int>> GetClearedPuzzleIds(long viewerId);

    /// <summary>Returns cleared puzzle_ids grouped by their PuzzleEntry.GroupId. Only groups
    /// with at least one clear appear in the dictionary.</summary>
    Task<Dictionary<int, HashSet<int>>> GetClearedPuzzleIdsByGroup(long viewerId);

    /// <summary>Inserts or updates the (viewer, puzzle) clear row. BestRetryCount keeps the
    /// minimum retry_count across all wins.</summary>
    Task UpsertClearAsync(long viewerId, int puzzleId, int retryCount);
}
