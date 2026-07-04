using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Entities.Story;

public class StorySection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public int? WorldId { get; set; }
    public StoryWorld? World { get; set; }

    public StoryApiType StoryApiType { get; set; }
    public int OrderId { get; set; }
    public int AllStoryOrderId { get; set; }
    public string NameTextKey { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public bool IsLeaderSelect { get; set; }
    public int BackGroundId { get; set; }
    public int ChapterSelectType { get; set; }
    public int StoryTypeOverwrite { get; set; }
    public bool IsUnderMaintenance { get; set; }
    public bool IsPlayAnotherEndAppearanceAnimation { get; set; }

    public int IsSpoiler { get; set; }
    public string SpoilerMessage { get; set; } = string.Empty;
}
