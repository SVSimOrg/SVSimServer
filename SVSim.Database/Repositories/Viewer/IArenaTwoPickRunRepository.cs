using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Viewer;

public interface IArenaTwoPickRunRepository
{
    Task<ViewerArenaTwoPickRun?> GetByViewerIdAsync(long viewerId);
    Task UpsertAsync(ViewerArenaTwoPickRun run);
    Task DeleteAsync(long viewerId);
}
