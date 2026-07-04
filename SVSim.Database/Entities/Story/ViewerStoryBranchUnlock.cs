namespace SVSim.Database.Entities.Story;

// Composite PK (ViewerId, StoryId) — StoryId here is the BRANCH CHILD that was unlocked.
public class ViewerStoryBranchUnlock
{
    public long ViewerId { get; set; }
    public int StoryId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
