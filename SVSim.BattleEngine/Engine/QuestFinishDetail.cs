using System.Collections.Generic;
using LitJson;
using Wizard;

public class QuestFinishDetail
{
	public enum WinBonusStatus
	{
	}

	public class MissionClearInfo
	{
		public string MissionText { get; private set; }

		public int MissionPoint { get; private set; }

		public MissionClearInfo(string missionText, int missionPoint)
		{
			MissionText = missionText;
			MissionPoint = missionPoint;
		}
	}

	public int MaxLife { get; set; }

	public List<UserMission> achieved_mission_list => AchievedInfo._missions;

	public List<UserAchievement> achieved_achievement_list => AchievedInfo._achievements;

	public AchievedInfo AchievedInfo { get; private set; }

	public QuestFinishDetail()
	{
		AchievedInfo = new AchievedInfo();
	}
}
