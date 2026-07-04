using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public interface IArenaTwoPickRewardRepository
{
    Task<List<ArenaTwoPickReward>> GetRewardsByWinCountAsync(int winCount);
    Task<int> GetMaxWinCountAsync();
}
