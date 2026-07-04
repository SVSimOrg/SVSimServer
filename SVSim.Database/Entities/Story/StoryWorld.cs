using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Entities.Story;

public class StoryWorld
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string TitleTextKey { get; set; } = string.Empty;
    public string PanelImageName { get; set; } = string.Empty;
    public string RibbonText { get; set; } = string.Empty;
}
