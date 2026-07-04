using SVSim.Database.Common;

namespace SVSim.Database.Models;

public class ClassExpEntry : BaseEntity<int>
{
    public int NecessaryExp { get; set; }
}