using System.Collections.Generic;
using LitJson;
using Wizard;

public class MatchFinishBase
{
	public JsonData _responseData;

	public BattleManagerBase.BATTLE_RESULT_TYPE battleResult;

	public int get_class_chara_experience;

	public int class_chara_experience;

	public int class_chara_level;

	public bool IsProcessed;

	public List<UserMission> achieved_mission_list => AchievedInfo._missions;

	public List<UserAchievement> achieved_achievement_list => AchievedInfo._achievements;

	public AchievedInfo AchievedInfo { get; private set; }

	public TreasureBoxCpResultInfo TreasureBoxCpResultInfo { get; private set; }

	public MatchFinishBase()
	{
		get_class_chara_experience = 0;
		class_chara_experience = 0;
		class_chara_level = 0;
		AchievedInfo = new AchievedInfo();
		TreasureBoxCpResultInfo = new TreasureBoxCpResultInfo();
	}
}
