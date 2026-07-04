using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class ShadowverseCardSetEntry : BaseEntity<int>
{
    /// <summary>
    /// The internal name of the set.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The cards in the set.
    /// </summary>
    public List<ShadowverseCardEntry> Cards { get; set; } = new List<ShadowverseCardEntry>();

    public bool IsInRotation { get; set; }
    public bool IsBasic { get; set; }
}
