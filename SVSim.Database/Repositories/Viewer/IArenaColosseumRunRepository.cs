using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Viewer;

public interface IArenaColosseumRunRepository
{
    Task<ViewerArenaColosseumRun?> GetByViewerIdAsync(long viewerId);
    Task UpsertAsync(ViewerArenaColosseumRun run);
    Task DeleteAsync(long viewerId);
}
