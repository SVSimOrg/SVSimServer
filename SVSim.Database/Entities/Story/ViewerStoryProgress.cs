namespace SVSim.Database.Entities.Story;

// Composite PK (ViewerId, StoryId) configured via fluent API in SVSimDbContext.
public class ViewerStoryProgress
{
    public long ViewerId { get; set; }
    public int StoryId { get; set; }

    public bool IsFinish { get; set; }
    public bool IsSkipped { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime? SkippedAt { get; set; }
}
