using SVSim.Database.Enums;

namespace SVSim.Database.Models.Config;

/// <summary>
/// Daily login bonus catalog. The Normal cycle is the always-on N-day streak. Total
/// (continuous milestones) and Campaign (date-bounded specials) are placeholders — wire
/// emits empty arrays for them until a campaign is captured and seeded here.
/// </summary>
[ConfigSection("LoginBonus")]
public class LoginBonusConfig
{
    public int CampaignId { get; set; } = 3;
    public string Name { get; set; } = "Daily Bonus";

    /// <summary>BG image asset key — wire ships as string. Prod captured value is "0".</summary>
    public string Img { get; set; } = "0";

    /// <summary>
    /// Normal-cycle reward entries, 1-based by Day. Defaults to <c>new()</c> (empty);
    /// real entries live in <see cref="ShippedDefaults"/>. <b>Do not move them into this
    /// property initializer</b> — collection initializers silently empty out under
    /// <c>GameConfigService.Get&lt;T&gt;()</c>'s tier-merge (config-defaults convention,
    /// historical bug 2026-05-24).
    /// </summary>
    public List<NormalBonusEntry> Normal { get; set; } = new();

    public static LoginBonusConfig ShippedDefaults() => new()
    {
        CampaignId = 3,
        Name = "Daily Bonus",
        Img = "0",
        Normal = new List<NormalBonusEntry>
        {
            new() { Day = 1,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 20 },
            new() { Day = 2,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 20 },
            new() { Day = 3,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 20 },
            new() { Day = 4,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 20 },
            new() { Day = 5,  EffectId = 2, RewardType = UserGoodsType.Item, RewardDetailId = 80001, RewardNumber = 1  },
            new() { Day = 6,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 30 },
            new() { Day = 7,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 30 },
            new() { Day = 8,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 30 },
            new() { Day = 9,  EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 30 },
            new() { Day = 10, EffectId = 2, RewardType = UserGoodsType.Item, RewardDetailId = 80001, RewardNumber = 1  },
            new() { Day = 11, EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 40 },
            new() { Day = 12, EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 40 },
            new() { Day = 13, EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 40 },
            new() { Day = 14, EffectId = 1, RewardType = UserGoodsType.Rupy, RewardDetailId = 0,     RewardNumber = 40 },
            new() { Day = 15, EffectId = 2, RewardType = UserGoodsType.Item, RewardDetailId = 1,     RewardNumber = 1  },
        },
    };
}
