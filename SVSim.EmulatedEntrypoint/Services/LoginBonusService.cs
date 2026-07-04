using System.Globalization;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Services;

public class LoginBonusService : ILoginBonusService
{
    private readonly IGameConfigService _config;
    private readonly TimeProvider _time;
    private readonly IGameCalendarService _calendar;

    public LoginBonusService(IGameConfigService config, TimeProvider time, IGameCalendarService calendar)
    {
        _config = config;
        _time = time;
        _calendar = calendar;
    }

    public bool IsDue(Viewer viewer) =>
        _calendar.ResetReady(viewer.LastLoginBonusClaimedAt);

    public async Task<DailyLoginBonus?> GrantIfDueAsync(IInventoryTransaction tx, CancellationToken ct = default)
    {
        var viewer = tx.Viewer;
        if (!IsDue(viewer)) return null;

        var cfg = _config.Get<LoginBonusConfig>();
        if (cfg.Normal.Count == 0) return null; // catalog misconfigured — nothing to grant

        int newStreak = (viewer.LoginBonusStreak % cfg.Normal.Count) + 1;
        var entry = cfg.Normal[newStreak - 1];

        await tx.GrantAsync(entry.RewardType, entry.RewardDetailId, entry.RewardNumber, ct);

        viewer.LoginBonusStreak = newStreak;
        viewer.LastLoginBonusClaimedAt = _time.GetUtcNow().UtcDateTime;

        return new DailyLoginBonus
        {
            Normal = new LoginBonusCampaign
            {
                Name = cfg.Name,
                CampaignId = cfg.CampaignId.ToString(CultureInfo.InvariantCulture),
                Img = cfg.Img,
                NowCount = newStreak,
                IsNextReward = true,
                IsOneDayMultiRewards = false,
                Rewards = cfg.Normal.Select(n => new LoginBonusReward
                {
                    EffectId       = n.EffectId.ToString(CultureInfo.InvariantCulture),
                    RewardType     = ((int)n.RewardType).ToString(CultureInfo.InvariantCulture),
                    RewardDetailId = n.RewardDetailId.ToString(CultureInfo.InvariantCulture),
                    RewardNumber   = n.RewardNumber.ToString(CultureInfo.InvariantCulture),
                }).ToList(),
            },
            Total = null,
            Campaign = new List<LoginBonusCampaign>(),
        };
    }
}
