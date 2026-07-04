using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class ViewerClassData
{
    public int Level { get; set; }
    public int Exp { get; set; }

    /// <summary>
    /// Per-class "use random leader skin from owned pool" preference. Defaults to false.
    /// No client-side setter exists today (only per-deck random-leader-skin endpoints exist);
    /// persisted now so when/if a class-level toggle is discovered, the write target exists.
    /// </summary>
    public bool IsRandomLeaderSkin { get; set; }

    #region Navigation Properties

    public ClassEntry Class { get; set; } = new ClassEntry();

    public Viewer Viewer { get; set; } = new Viewer();

    public LeaderSkinEntry LeaderSkin { get; set; } = new LeaderSkinEntry();

    #endregion
}