using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class SleeveEntry : BaseEntity<int>
{
    #region Navigation Properties

    public List<Viewer> Viewers = new List<Viewer>();

    #endregion

}