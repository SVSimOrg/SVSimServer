using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class ClassEntry : BaseEntity<int>
{
    /// <summary>
    /// The name of the class.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #region Navigation Properties

    public List<LeaderSkinEntry> LeaderSkins { get; set; } = new List<LeaderSkinEntry>();

    [NotMapped] 
    public LeaderSkinEntry? DefaultLeaderSkin => LeaderSkins.FirstOrDefault(skin => skin.Id == Id);

    #endregion

}