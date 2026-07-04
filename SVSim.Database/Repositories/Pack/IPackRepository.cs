using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Pack;

public interface IPackRepository
{
    Task<List<PackConfigEntry>> GetActivePacks(DateTime now);
    Task<PackConfigEntry?> GetPack(int parentGachaId);
    Task<Dictionary<int, ViewerPackOpenCount>> GetOpenCountsForViewer(long viewerId);
    Task IncrementOpenCount(long viewerId, int parentGachaId, int by);
    Task MarkDailyFreeUsed(long viewerId, int parentGachaId, DateTime when);
}
