using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class LeaderSkinEntry : BaseEntity<int>
{
    // Name of the skin
    public string Name { get; set; } = string.Empty;
    
    // ID of the emote associated with the skin
    public int EmoteId { get; set; }

    #region Foreign Keys

    public int? ClassId { get; set; }

    #endregion

    #region Navigation Properties

    public ClassEntry? Class { get; set; }

    public List<Viewer> Viewers { get; set; } = new List<Viewer>();

    #endregion
}