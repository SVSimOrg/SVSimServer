using SVSim.Database.Enums;

namespace SVSim.Database.Entities.Story;

[Microsoft.EntityFrameworkCore.Owned]
public class StoryChapterReward
{
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }
}
