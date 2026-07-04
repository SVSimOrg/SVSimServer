using System.Collections.Generic;
using LitJson;

public class PracticePuzzleFinishData
{

	public List<UserMission> achieved_mission_list => AchievedInfo._missions;

	public List<UserAchievement> achieved_achievement_list => AchievedInfo._achievements;

	public AchievedInfo AchievedInfo { get; private set; }

	public PracticePuzzleFinishData()
	{
		AchievedInfo = new AchievedInfo();
	}
}
