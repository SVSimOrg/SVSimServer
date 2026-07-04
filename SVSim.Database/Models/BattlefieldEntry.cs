using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class BattlefieldEntry : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
}