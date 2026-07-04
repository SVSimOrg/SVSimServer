using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard.AutoTest;
using Wizard.Story;

namespace Wizard.Battle.Recovery;

public class SetupConditionInfo : BattleConditionInfo
{
	public bool DidPlayerGoFirst { get; private set; }

	public int RandomSeed { get; private set; }

	public int BackGroundId { get; private set; }

	public string BgmId { get; private set; }

	public bool HasMulliganInfo { get; private set; }

	public IEnumerable<int> PlayerMulliganReplaceCards { get; private set; }

	public IEnumerable<int> PlayerMulliganCompleteCards { get; private set; }

	public IEnumerable<int> EnemyMulliganReplaceCards { get; private set; }

	public IEnumerable<int> EnemyMulliganCompleteCards { get; private set; }

	public int PracticeDifficultyDegreeId { get; private set; }

	public bool IsPrebuildDeck { get; private set; }

	public bool IsTrialDeck { get; private set; }

	public bool IsDefaultDeck { get; private set; }

	public int QuestStageId { get; private set; }

	public int QuestEnemyAiId { get; private set; }

	public int QuestEnemyEmblemId { get; private set; }

	public int QuestEnemyDegreeId { get; private set; }

	public int RecoveryPoint { get; private set; }

	public List<BossRushSpecialSkill> QuestPlayerSkillList { get; private set; }

	public BossRushSpecialSkill QuestEnemySkill { get; private set; }

	public int QuestMaxBattleCount { get; private set; }

	public int QuestCurrentWinCount { get; private set; }

	public int QuestEnemyEmotionOverride { get; private set; }

	public int QuestPlayerEmotionOverride { get; private set; }

	public bool QuestIsExtra { get; private set; }

	public bool QuestIsMockBattle { get; private set; }

	public int QuestExtraDeckScheduleId { get; private set; }

	public BattleManagerBase.MissionNecessaryInformation MissionNecessaryInformation { get; private set; } = new BattleManagerBase.MissionNecessaryInformation(new Dictionary<string, string>());

	public StoryRecoveryData StoryRecoveryData { get; }

	public bool ScenarioDeckIsPreBuild { get; private set; }

	public bool ScenarioDeckIsTrial { get; private set; }

	public SetupConditionInfo(JsonData jsonData, DataMgr.BattleType battleType)
		: base(jsonData, useDefaultInPlayCardValue: true)
	{
		DidPlayerGoFirst = jsonData.ToBooleanOrDefault("player_start_turn", defaultBoolean: true);
		RandomSeed = jsonData.ToIntOrDefault("random_seed", 0);
		BackGroundId = jsonData.ToIntOrDefault("background_id", -1);
		BgmId = jsonData.ToStringOrDefault("bgm_id", "NONE");
		HasMulliganInfo = jsonData.HasKey("player_mulligan_abandon");
		PracticeDifficultyDegreeId = jsonData.ToIntOrDefault("practice_difficulty_degree_id", 400001);
		IsPrebuildDeck = jsonData.ToBooleanOrDefault("is_prebuild_deck", defaultBoolean: false);
		IsTrialDeck = jsonData.ToBooleanOrDefault("is_trial_deck", defaultBoolean: false);
		IsDefaultDeck = jsonData.ToBooleanOrDefault("is_default_deck", defaultBoolean: false);
		QuestStageId = jsonData.ToIntOrDefault("quest_stage_id", 0);
		QuestEnemyAiId = jsonData.ToIntOrDefault("quest_enemy_ai_id", 0);
		QuestEnemyEmblemId = jsonData.ToIntOrDefault("quest_enemy_emblem_id", 0);
		QuestEnemyDegreeId = jsonData.ToIntOrDefault("quest_enemy_degree_id", 0);
		QuestEnemyEmotionOverride = jsonData.ToIntOrDefault("quest_enemy_emotion_override", 0);
		QuestPlayerEmotionOverride = jsonData.ToIntOrDefault("quest_player_emotion_override", 0);
		QuestIsExtra = jsonData.ToBooleanOrDefault("quest_is_extra", defaultBoolean: false);
		QuestIsMockBattle = jsonData.ToBooleanOrDefault("quest_is_mock_battle", defaultBoolean: false);
		QuestExtraDeckScheduleId = jsonData.ToIntOrDefault("quest_extra_deck_schedule_id", 0);
		if ((battleType == DataMgr.BattleType.Story || DataMgr.IsQuestBattleType(battleType)) && jsonData.HasKey("story_special_battle_player_attach_skill"))
		{
			/* Pre-Phase-5b: SetSpecialBattleSetting is Story/Quest-only; headless is PvP-only */
		}
		if (battleType == DataMgr.BattleType.BossRushQuest)
		{
			RecoveryPoint = jsonData.ToIntOrDefault("recovery_point", 0);
			QuestPlayerSkillList = (from d in jsonData.ToJsonDataCollection("quest_player_skill_list")
				select new BossRushSpecialSkill(d)).ToList();
			QuestEnemySkill = new BossRushSpecialSkill(jsonData.ToObjectOrNull("quest_enemy_skill"));
			QuestMaxBattleCount = jsonData.ToIntOrDefault("quest_max_battle_count", 0);
			QuestCurrentWinCount = jsonData.ToIntOrDefault("quest_current_win_count", 0);
		}
		if (battleType == DataMgr.BattleType.SecretBossQuest)
		{
			QuestPlayerSkillList = (from d in jsonData.ToJsonDataCollection("quest_player_skill_list")
				select new BossRushSpecialSkill(d)).ToList();
			QuestEnemySkill = new BossRushSpecialSkill(jsonData.ToObjectOrNull("quest_enemy_skill"));
		}
		if (jsonData.HasKey("mission_necessary_information"))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string key in jsonData["mission_necessary_information"].Keys)
			{
				dictionary.Add(key, jsonData["mission_necessary_information"].ToStringOrDefault(key, string.Empty));
			}
			MissionNecessaryInformation = new BattleManagerBase.MissionNecessaryInformation(dictionary);
		}
		switch (battleType)
		{
		case DataMgr.BattleType.Practice:
			/* Pre-Phase-5b: Practice3DfieldId is Practice-only; headless is PvP-only */
			break;
		case DataMgr.BattleType.Story:
			StoryRecoveryData = new StoryRecoveryData(jsonData);
			ScenarioDeckIsPreBuild = jsonData.ToBooleanOrDefault("scenario_deck_is_pre_build", defaultBoolean: false);
			ScenarioDeckIsTrial = jsonData.ToBooleanOrDefault("scenario_deck_is_trial", defaultBoolean: false);
			break;
		}
		if (HasMulliganInfo)
		{
			PlayerMulliganReplaceCards = ParseJsonDataIndexes(jsonData, "player_mulligan_abandon");
			PlayerMulliganCompleteCards = ParseJsonDataIndexes(jsonData, "player_mulligan_complete");
			EnemyMulliganReplaceCards = ParseJsonDataIndexes(jsonData, "enemy_mulligan_abandon");
			EnemyMulliganCompleteCards = ParseJsonDataIndexes(jsonData, "enemy_mulligan_complete");
		}
	}

	private IEnumerable<int> ParseJsonDataIndexes(JsonData jsonData, string keyName)
	{
		return from j in jsonData.ToJsonDataCollection(keyName)
			select int.Parse(j.ToString().Substring(1));
	}
}
