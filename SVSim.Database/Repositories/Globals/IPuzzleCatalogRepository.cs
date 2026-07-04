using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public interface IPuzzleCatalogRepository
{
    Task<List<PuzzleGroupEntry>> GetAllGroupsWithPuzzles();
    Task<PuzzleGroupEntry?>      GetGroupWithPuzzles(int puzzleMasterId);
    Task<List<PuzzleMissionEntry>> GetAllMissionsOrdered();
}
