using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Globals;

public class ArenaTwoPickRewardRepository : IArenaTwoPickRewardRepository
{
    private readonly SVSimDbContext _db;
    public ArenaTwoPickRewardRepository(SVSimDbContext db) => _db = db;

    public async Task<List<ArenaTwoPickReward>> GetRewardsByWinCountAsync(int winCount) =>
        await _db.ArenaTwoPickRewards
            .Where(r => r.WinCount == winCount)
            .ToListAsync();

    public async Task<int> GetMaxWinCountAsync()
    {
        if (!await _db.ArenaTwoPickRewards.AnyAsync()) return 0;
        return await _db.ArenaTwoPickRewards.MaxAsync(r => r.WinCount);
    }
}
