namespace SVSim.Database.Entities.Story;

[Microsoft.EntityFrameworkCore.Owned]
public class StoryChapterBattleSetting
{
    public int DeckClassId { get; set; }
    public int PlayerEmotionOverride { get; set; }
    public int EnemyEmotionOverride { get; set; }
    public int SkinIdOverride { get; set; }
    public int Battle3dFieldIdOverride { get; set; }
    public int BgmIdOverride { get; set; }
    public int DeckSkinIdOverride { get; set; }
}
