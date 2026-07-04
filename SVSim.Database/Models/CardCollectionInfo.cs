using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class CardCollectionInfo
{
    public int CraftCost { get; set; }
    public int DustReward { get; set; }
}