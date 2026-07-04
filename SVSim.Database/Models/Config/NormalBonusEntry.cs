using SVSim.Database.Enums;

namespace SVSim.Database.Models.Config;

public class NormalBonusEntry
{
    public int Day { get; set; }
    public int EffectId { get; set; } = 1;
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
}
