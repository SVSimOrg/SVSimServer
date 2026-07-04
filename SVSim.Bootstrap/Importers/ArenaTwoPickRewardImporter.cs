using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of <see cref="ArenaTwoPickReward"/> rows from
/// <c>arena-two-pick-rewards.json</c>. Key = (WinCount, RewardGroup, RewardType, RewardId, RewardNum).
/// </summary>
public class ArenaTwoPickRewardImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var path = Path.Combine(seedDir, "arena-two-pick-rewards.json");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[ArenaTwoPickRewardImporter] missing {path}; skipping.");
            return 0;
        }

        var seeds = SeedLoader.LoadList<ArenaTwoPickRewardSeed>(path);
        var existing = await context.ArenaTwoPickRewards
            .ToDictionaryAsync(r => (r.WinCount, r.RewardGroup, r.RewardType, r.RewardId, r.RewardNum));

        int upserted = 0;
        foreach (var s in seeds)
        {
            if (existing.TryGetValue((s.WinCount, s.RewardGroup, (UserGoodsType)s.RewardType, s.RewardId, s.RewardNum), out var row))
            {
                row.Weight = s.Weight;
            }
            else
            {
                context.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
                {
                    WinCount    = s.WinCount,
                    RewardGroup = s.RewardGroup,
                    Weight      = s.Weight,
                    RewardType  = (UserGoodsType)s.RewardType,
                    RewardId    = s.RewardId,
                    RewardNum   = s.RewardNum,
                });
            }
            upserted++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[ArenaTwoPickRewardImporter] upserted={upserted}");
        return upserted;
    }
}
