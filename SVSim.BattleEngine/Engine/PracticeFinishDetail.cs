using System.Collections.Generic;
using LitJson;

public class PracticeFinishDetail
{
	public JsonData _responseData;

	public int get_class_chara_experience;

	public int class_chara_experience;

	public int class_chara_level;

	public List<UserMission> achieved_mission_list => AchievedInfo._missions;

	public List<UserAchievement> achieved_achievement_list => AchievedInfo._achievements;

	public AchievedInfo AchievedInfo { get; private set; }

	public PracticeFinishDetail()
	{
		AchievedInfo = new AchievedInfo();
	}
}
