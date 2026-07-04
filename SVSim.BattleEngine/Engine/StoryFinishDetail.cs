using System;
using System.Collections.Generic;
using LitJson;

public class StoryFinishDetail
{
	public JsonData _responseData;

	public int get_class_chara_experience;

	public int class_chara_experience;

	public int class_chara_level;

	public List<UserMission> achieved_mission_list => AchievedInfo._missions;

	public List<UserAchievement> achieved_achievement_list => AchievedInfo._achievements;

	public IReadOnlyList<ReceivedReward> StoryClearRewards { get; set; } = Array.Empty<ReceivedReward>();

	public AchievedInfo AchievedInfo { get; private set; }

	public StoryFinishDetail()
	{
		AchievedInfo = new AchievedInfo();
	}
}
