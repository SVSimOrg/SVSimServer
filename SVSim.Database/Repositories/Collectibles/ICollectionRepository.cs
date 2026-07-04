using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Collectibles;

public interface ICollectionRepository
{
    Task<List<LeaderSkinEntry>> GetLeaderSkins();
    Task<List<int>> GetAllSleeveIds();
    Task<List<int>> GetAllEmblemIds();
    Task<List<int>> GetAllDegreeIds();
    Task<List<int>> GetAllMyPageBackgroundIds();
}