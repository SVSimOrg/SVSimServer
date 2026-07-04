using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class OwnedItemEntry
{
    public int Count { get; set; }

    #region Navigation Properties

    public ItemEntry Item { get; set; } = new ItemEntry();

    public Viewer Viewer { get; set; } = new Viewer();

    #endregion
}