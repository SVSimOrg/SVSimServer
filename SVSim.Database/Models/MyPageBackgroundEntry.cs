using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class MyPageBackgroundEntry : BaseEntity<int>
{
    #region Navigation Properties

    public List<Viewer> Viewers { get; set; } = new List<Viewer>();

    #endregion
}