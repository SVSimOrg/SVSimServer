using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class ViewerMissionData
{
    public bool HasReceivedPickTwoMission { get; set; }
    public int MissionReceiveType { get; set; }
    public DateTime MissionChangeTime { get; set; }
    public int TutorialState { get; set; }
}