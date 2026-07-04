namespace SVSim.Database.Entities.Story;

[Microsoft.EntityFrameworkCore.Owned]
public class StorySubChapter
{
    public int SubChapterId { get; set; }
    public int SubChapterStoryId { get; set; }
    public bool IsMaintenanceChapter { get; set; }
}
