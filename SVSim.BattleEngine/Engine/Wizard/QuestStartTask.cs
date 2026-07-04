using LitJson;

namespace Wizard;

public class QuestStartTask : BaseTask
{
	public class QuestStartTaskParam : BaseParam
	{
		public int quest_stage_id;

		public int extra_deck_schedule_id;
	}

	public class QuestStartTaskParamForPuzzle : BaseParam
	{
		public int puzzle_id;
	}

	public QuestStartTask(bool isPuzzle = false)
	{
		base.type = (isPuzzle ? ApiType.Type.QuestPuzzleStart : ApiType.Type.QuestStart);
	}

	public void SetParameterForPuzzle(int puzzleId)
	{
		base.Params = new QuestStartTaskParamForPuzzle
		{
			puzzle_id = puzzleId
		};
	}

	protected override int Parse()
	{
		int num = base.Parse();
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		dataMgr.ClearSpecialBattleSettingInfo();
		if (num != 1)
		{
			return num;
		}
		if (base.type == ApiType.Type.QuestPuzzleStart)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["special_battle_setting"];
		int num2 = jsonData["player_first_turn"].ToInt();
		dataMgr.SetSpecialBattleSetting((num2 == 0) ? ((bool?)null) : new bool?(num2 == 1), playerPp: jsonData["player_start_pp"].ToInt(), enemyPp: jsonData["enemy_start_pp"].ToInt(), playerLife: jsonData["player_start_life"].ToInt(), playerMaxLife: jsonData["player_start_life"].ToInt(), enemyMaxLife: jsonData["enemy_start_life"].ToInt(), playerSkill: jsonData["player_attach_skill"].ToString(), enemySkill: jsonData["enemy_attach_skill"].ToString(), idOverrideBattleLogText: jsonData["id_override_in_battle_log"].ToString());
		dataMgr.SetMissionNecessaryInformation(base.ResponseData["data"]["mission_parameter"]);
		return num;
	}
}
