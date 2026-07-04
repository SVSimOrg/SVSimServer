using System;
using System.Collections.Generic;
using LitJson;
using Wizard.Battle.View.Vfx;
using Wizard.Story;

namespace Wizard.Battle.Recovery;

public class SingleBattleOperationRecorder : OperationRecorderBase
{
	protected DataMgr.BattleType _battleType;

	protected JsonData _setupJsonData = new JsonData();

	protected string _tempRecoveryFilePath = "";

	public SingleBattleOperationRecorder(string filePath, string tempFilePath = "")
		: base(filePath)
	{
		_tempRecoveryFilePath = tempFilePath;
		_setupJsonData["player"] = new JsonData();
		_setupJsonData["player"].SetJsonType(JsonType.Object);
		_setupJsonData["enemy"] = new JsonData();
		_setupJsonData["enemy"].SetJsonType(JsonType.Object);
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = null; // Pre-Phase-5b: SpecialBattleSettingInfo headless-null
		if (specialBattleSettingInfo != null)
		{
			RecordSpecialBattleSetting(specialBattleSettingInfo);
		}
		WriteJsonData();
	}

	public override void RecordBattleType(DataMgr.BattleType battleType)
	{
		_battleType = battleType;
		WriteJsonData();
	}

	public override void RecordRandomSeed(int randomSeed)
	{
		_setupJsonData["random_seed"] = randomSeed;
		WriteJsonData();
	}

	public override void RecordBackGroundId(int backGroundId)
	{
		_setupJsonData["background_id"] = backGroundId;
		WriteJsonData();
	}

	public override void RecordBgmId(string bgmId)
	{
		_setupJsonData["bgm_id"] = bgmId;
		WriteJsonData();
	}

	public override void RecordClass(string playerName, int clanType)
	{
		_setupJsonData[playerName]["clan_type"] = clanType;
		WriteJsonData();
	}

	public override void RecordSubClass(string playerName, int clanType)
	{
		_setupJsonData[playerName]["sub_class_type"] = clanType;
		WriteJsonData();
	}

	public override void RecordMyRotationId(string playerName, string myRotationId)
	{
		_setupJsonData[playerName]["my_rotation_id"] = myRotationId;
		WriteJsonData();
	}

	public override void RecordSleeve(string playerName, long sleeveId)
	{
		_setupJsonData[playerName]["sleeve_id"] = sleeveId;
		WriteJsonData();
	}

	public override void RecordChara(string playerName, int charaId)
	{
		_setupJsonData[playerName]["chara_id"] = charaId;
		WriteJsonData();
	}

	public override void RecordDeck(string playerName, char indexHeadChar, IEnumerable<int> cardIds)
	{
		JsonData jsonData = new JsonData();
		_setupJsonData[playerName]["deck"] = jsonData;
		jsonData.SetJsonType(JsonType.Array);
		int num = 1;
		foreach (int cardId in cardIds)
		{
			JsonData jsonData2 = new JsonData();
			jsonData2.SetJsonType(JsonType.Object);
			jsonData2["index"] = indexHeadChar.ToString() + num;
			jsonData2["id"] = cardId;
			jsonData2["name"] = CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId).CardName;
			jsonData.Add(jsonData2);
			num++;
		}
		WriteJsonData();
	}

	public override void RecordEnemyAIDifficulty(int level)
	{
		_setupJsonData["enemy"]["ai_difficulty"] = level;
		WriteJsonData();
	}

	public override void RecordEnemyAILogicLevel(int level)
	{
		_setupJsonData["enemy"]["ai_logic_level"] = level;
		WriteJsonData();
	}

	public override void RecordEnemyAIMaxLife(int life)
	{
		_setupJsonData["enemy"]["ai_max_life"] = life;
		WriteJsonData();
	}

	public override void RecordEnemyAIDeckId(int deckId)
	{
		_setupJsonData["enemy"]["ai_deck_id"] = deckId;
		WriteJsonData();
	}

	public override void RecordEnemyAIStyleId(int styleId)
	{
		_setupJsonData["enemy"]["ai_style_id"] = styleId;
		WriteJsonData();
	}

	public override void RecordEnemyAIEmoteId(int emoteId)
	{
		_setupJsonData["enemy"]["ai_emote_id"] = emoteId;
		WriteJsonData();
	}

	public override void RecordEnemyAIUseInnerEmote(bool useInnerEmote)
	{
		_setupJsonData["enemy"]["ai_use_inner_emote"] = useInnerEmote;
		WriteJsonData();
	}

	public override void RecordStartTurnIsPlayer(bool startTurnIsPlayer)
	{
		_setupJsonData["player_start_turn"] = startTurnIsPlayer;
		WriteJsonData();
	}

	public override void RecordPracticeDifficultyDegreeId(int degreeId)
	{
		_setupJsonData["practice_difficulty_degree_id"] = degreeId;
		WriteJsonData();
	}

	public override void RecordIsPreBuildDeck(bool isPreBuildDeck)
	{
		_setupJsonData["is_prebuild_deck"] = isPreBuildDeck;
		WriteJsonData();
	}

	public override void RecordIsTrialDeck(bool isTrialDeck)
	{
		_setupJsonData["is_trial_deck"] = isTrialDeck;
		WriteJsonData();
	}

	public override void RecordIsDefaultDeck(bool isDefaultDeck)
	{
		_setupJsonData["is_default_deck"] = isDefaultDeck;
		WriteJsonData();
	}

	public override void RecordQuestStageId(int id)
	{
		_setupJsonData["quest_stage_id"] = id;
		WriteJsonData();
	}

	public override void RecordQuestEnemyAiId(int id)
	{
		_setupJsonData["quest_enemy_ai_id"] = id;
		WriteJsonData();
	}

	public override void RecordQuestEnemyEmblemId(int id)
	{
		_setupJsonData["quest_enemy_emblem_id"] = id;
		WriteJsonData();
	}

	public override void RecordQuestEnemyDegreeId(int id)
	{
		_setupJsonData["quest_enemy_degree_id"] = id;
		WriteJsonData();
	}

	public override void RecordQuestEnemyEmotionOverride(int id)
	{
		_setupJsonData["quest_enemy_emotion_override"] = id;
		WriteJsonData();
	}

	public override void RecordQuestPlayerEmotionOverride(int id)
	{
		_setupJsonData["quest_player_emotion_override"] = id;
		WriteJsonData();
	}

	public override void RecordQuestRecoveryPoint(int recoveryPoint)
	{
		_setupJsonData["recovery_point"] = recoveryPoint;
		WriteJsonData();
	}

	public override void RecordQuestPlayerSkillList(List<BossRushSpecialSkill> skills)
	{
		JsonData jsonData = new JsonData();
		_setupJsonData["quest_player_skill_list"] = jsonData;
		jsonData.SetJsonType(JsonType.Array);
		foreach (BossRushSpecialSkill skill in skills)
		{
			JsonData jsonData2 = new JsonData();
			jsonData2.SetJsonType(JsonType.Object);
			jsonData2["original_card_id"] = skill.OriginalCardId;
			jsonData2["name"] = skill.Name;
			jsonData2["skill_text"] = skill.SkillText;
			jsonData2["skill_desc_text"] = skill.SkillDescText;
			jsonData2["is_foil"] = skill.IsFoil;
			jsonData.Add(jsonData2);
		}
		WriteJsonData();
	}

	public override void RecordQuestEnemySkill(BossRushSpecialSkill skill)
	{
		JsonData jsonData = new JsonData();
		jsonData.SetJsonType(JsonType.Object);
		jsonData["original_card_id"] = skill.OriginalCardId;
		jsonData["name"] = skill.Name;
		jsonData["skill_text"] = skill.SkillText;
		jsonData["skill_desc_text"] = skill.SkillDescText;
		jsonData["is_foil"] = skill.IsFoil;
		_setupJsonData["quest_enemy_skill"] = jsonData;
		WriteJsonData();
	}

	public override void RecordQuestMaxBattleCount(int maxBattleCount)
	{
		_setupJsonData["quest_max_battle_count"] = maxBattleCount;
		WriteJsonData();
	}

	public override void RecordQuestCurrentWinCount(int currentWinCount)
	{
		_setupJsonData["quest_current_win_count"] = currentWinCount;
		WriteJsonData();
	}

	public override void RecordQuestIsExtra(bool isExtra)
	{
		_setupJsonData["quest_is_extra"] = isExtra;
		WriteJsonData();
	}

	public override void RecordQuestIsMockBattle(bool isMockBattle)
	{
		_setupJsonData["quest_is_mock_battle"] = isMockBattle;
		WriteJsonData();
	}

	public override void RecordQuestExtraDeckScheduleId(int id)
	{
		_setupJsonData["quest_extra_deck_schedule_id"] = id;
		WriteJsonData();
	}

	public override void RecordMissionNecessaryInformation(BattleManagerBase.MissionNecessaryInformation missionNecessaryInformation)
	{
		_setupJsonData["mission_necessary_information"] = new JsonData();
		_setupJsonData["mission_necessary_information"].SetJsonType(JsonType.Object);
		foreach (KeyValuePair<string, string> item in missionNecessaryInformation.GetOriginalTargetDictionary())
		{
			_setupJsonData["mission_necessary_information"][item.Key] = item.Value;
		}
		WriteJsonData();
	}

	public override void RecordStoryData()
	{
		JsonData jsonData = new StoryRecoveryData(Data.SelectedStoryInfo).ToJsonData();
		foreach (string key in jsonData.Keys)
		{
			_setupJsonData[key] = jsonData[key];
		}
		_setupJsonData["scenario_deck_is_pre_build"] = Data.StoryInfo.IsPreBuildDeck;
		_setupJsonData["scenario_deck_is_trial"] = Data.StoryInfo.IsTrialDeck;
		WriteJsonData();
	}

	public override void RecordMulliganStart()
	{
	}

	public override void RecordPractice3DFieldId(int id)
	{
		_setupJsonData["practice_3d_field_id"] = id;
		WriteJsonData();
	}

	public override void RecordPlayerMulliganReplaceCards(IEnumerable<BattleCardBase> replaceCards, IEnumerable<int> completeCards)
	{
		JsonData jsonData = new JsonData();
		jsonData.SetJsonType(JsonType.Array);
		foreach (BattleCardBase replaceCard in replaceCards)
		{
			jsonData.Add(replaceCard.GetName());
		}
		_setupJsonData["player_mulligan_abandon"] = jsonData;
		JsonData jsonData2 = new JsonData();
		jsonData2.SetJsonType(JsonType.Array);
		foreach (int completeCard in completeCards)
		{
			jsonData2.Add("p" + completeCard);
		}
		_setupJsonData["player_mulligan_complete"] = jsonData2;
		WriteJsonData();
	}

	public override void RecordEnemyMulliganReplaceCards(IEnumerable<BattleCardBase> replaceCards, IEnumerable<int> completeCards)
	{
		JsonData jsonData = new JsonData();
		jsonData.SetJsonType(JsonType.Array);
		foreach (BattleCardBase replaceCard in replaceCards)
		{
			jsonData.Add(replaceCard.GetName());
		}
		_setupJsonData["enemy_mulligan_abandon"] = jsonData;
		JsonData jsonData2 = new JsonData();
		jsonData2.SetJsonType(JsonType.Array);
		foreach (int completeCard in completeCards)
		{
			jsonData2.Add("e" + completeCard);
		}
		_setupJsonData["enemy_mulligan_complete"] = jsonData2;
		WriteJsonData();
	}

	public override void RecordPlay(BattleCardBase card, BattleCardBase originalCard, IEnumerable<BattleCardBase> selectedCards)
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "play";
		jsonData["index"] = CardToIndexName(card);
		jsonData["id"] = card.CardId;
		jsonData["name"] = card.BaseParameter.CardName;
		jsonData["cost"] = card.Cost;
		jsonData["is_choice_brave"] = (originalCard.IsChoiceBraveSkillCard ? 1 : 0);
		WriteSkillTargetInfoToJson(selectedCards, jsonData);
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	public override VfxBase RecordAttack(BattleCardBase attackCard, BattleCardBase targetCard, SkillProcessor skillProcessor)
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "attack";
		jsonData["index"] = CardToIndexName(attackCard);
		jsonData["id"] = attackCard.CardId;
		jsonData["name"] = attackCard.BaseParameter.CardName;
		jsonData["target"] = CardToIndexName(targetCard);
		jsonData["target_id"] = targetCard.CardId;
		jsonData["target_name"] = targetCard.BaseParameter.CardName;
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
		return NullVfx.GetInstance();
	}

	public override void RecordEvolve(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCards)
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "evolve";
		jsonData["index"] = CardToIndexName(card);
		jsonData["id"] = card.CardId;
		jsonData["name"] = card.BaseParameter.CardName;
		WriteSkillTargetInfoToJson(selectedCards, jsonData);
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	public override void RecordStartSelect(BattleCardBase card, bool isEvolve, List<BattleCardBase> selectableCards, bool isChoiceBrave)
	{
	}

	public override void RecordSelect(BattleCardBase card, bool isEvolve, BattleCardBase actCard, bool isBurialRite, bool isChoiceBrave)
	{
	}

	public override void RecordCompleteSelect(BattleCardBase card, bool isEvolve, BattleCardBase actCard, bool isChoiceBrave, bool isBurialRite)
	{
	}

	public override void RecordStartChoice(BattleCardBase card, bool isEvolve, List<BattleCardBase> choiceCards, bool isChoiceBrave)
	{
	}

	public override void RecordStartFusion(BattleCardBase card, List<BattleCardBase> selectableCards)
	{
	}

	public override void RecordSelectFusion(BattleCardBase card)
	{
	}

	public override void RecordCompleteChoice(BattleCardBase card, bool isEvolve, List<BattleCardBase> chosenCardList, BattleCardBase actCard, List<int> chosenCardIndexList, bool hasSelectionSkill, bool isChoiceBrave)
	{
	}

	public override void RecordCancelChoice(BattleCardBase card, bool isEvolve, bool isChoiceBrave)
	{
	}

	public override void RecordCancelFusion(BattleCardBase card)
	{
	}

	public override void RecordCancelSelect(BattleCardBase actCard, bool isEvolve, bool isChoiceBrave)
	{
	}

	public override void RecordTurnStart()
	{
	}

	public override void RecordTurnEnd()
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "turn_end";
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	public override void RecordRetire()
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "retire";
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	public void RecordChangeAI(string logicName, int operationQueueCount)
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "change_ai";
		jsonData["logic"] = logicName;
		jsonData["queue_count"] = operationQueueCount;
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	public override void RecordSkillTargets(IEnumerable<BattleCardBase> targetCards)
	{
		foreach (BattleCardBase targetCard in targetCards)
		{
			_skillTargetList.Add(targetCard.GetName());
		}
		WriteJsonData();
	}

	public void RecordSpecialBattleSetting(DataMgr.SpecialBattleSetting setting)
	{
		if (setting != null)
		{
			_setupJsonData["story_special_battle_player_attach_skill"] = setting.PlayerAttachSkillText;
			_setupJsonData["story_special_battle_enemy_attach_skill"] = setting.EnemyAttachSkillText;
			_setupJsonData["story_special_battle_player_start_pp"] = setting.PlayerStartPp;
			_setupJsonData["story_special_battle_enemy_start_pp"] = setting.EnemyStartPp;
			_setupJsonData["story_special_battle_player_current_life"] = setting.PlayerStartLife;
			_setupJsonData["story_special_battle_player_start_life"] = setting.PlayerStartMaxLife;
			_setupJsonData["story_special_battle_enemy_start_life"] = setting.EnemyStartMaxLife;
			_setupJsonData["story_special_battle_id_override_in_battle_log"] = setting.IdOverrideInBattleLogText;
			_setupJsonData["story_special_battle_id"] = setting.Id;
			_setupJsonData["story_special_battle_banish_effect_override"] = setting.BanishEffectOverRideText;
			_setupJsonData["story_special_battle_token_draw_effect_override"] = setting.TokenDrawEffectOverrideText;
			_setupJsonData["story_special_battle_special_token_draw_effect_override"] = setting.SpecialTokenDrawEffectOverrideText;
			_setupJsonData["story_special_battle_skip_result"] = 0; // Pre-Phase-5b: SkipStorySpecialBattleResult headless-0
			WriteJsonData();
		}
	}

	public override void RecordCompleteFusionSelect(BattleCardBase fusionCard, IEnumerable<BattleCardBase> ingredientCards)
	{
		JsonData jsonData = new JsonData();
		jsonData["ope"] = "comp_fusion";
		jsonData["index"] = CardToIndexName(fusionCard);
		jsonData["id"] = fusionCard.CardId;
		jsonData["name"] = fusionCard.BaseParameter.CardName;
		WriteSkillTargetInfoToJson(ingredientCards, jsonData);
		_operationJsonDataList.Add(jsonData);
		WriteJsonData();
	}

	protected override void WriteJsonData()
	{
		if (RecoveryManagerBase.failedRecoveryFlag)
		{
			return;
		}
		JsonData jsonData = new JsonData();
		jsonData["version"] = 0;
		jsonData["battle_type"] = (int)_battleType;
		jsonData["record_time"] = DateTime.Now.Ticks;
		jsonData["setup"] = _setupJsonData;
		jsonData["operations"] = new JsonData();
		jsonData["operations"].SetJsonType(JsonType.Array);
		foreach (JsonData operationJsonData in _operationJsonDataList)
		{
			jsonData["operations"].Add(operationJsonData);
		}
		jsonData["check"] = new JsonData();
		jsonData["check"]["player"] = new JsonData();
		jsonData["check"]["player"].SetJsonType(JsonType.Object);
		jsonData["check"]["enemy"] = new JsonData();
		jsonData["check"]["enemy"].SetJsonType(JsonType.Object);
		// Pre-Phase-5b: check-block state snapshot dropped headless (no mgr in scope in WriteJsonData)
		JsonData jsonData2 = new JsonData(); jsonData2.SetJsonType(JsonType.Array);
		JsonData jsonData3 = new JsonData(); jsonData3.SetJsonType(JsonType.Array);
		JsonData jsonData4 = new JsonData(); jsonData4.SetJsonType(JsonType.Array);
		JsonData jsonData5 = new JsonData(); jsonData5.SetJsonType(JsonType.Array);
		jsonData["check"]["player"]["hand"] = jsonData2;
		jsonData["check"]["player"]["inplay"] = jsonData3;
		jsonData["check"]["enemy"]["hand"] = jsonData4;
		jsonData["check"]["enemy"]["inplay"] = jsonData5;
		JsonData jsonData6 = new JsonData();
		jsonData6.SetJsonType(JsonType.Array);
		foreach (string skillTarget in _skillTargetList)
		{
			jsonData6.Add(skillTarget);
		}
		if (jsonData6.Count > 0)
		{
			jsonData["skill_targets"] = jsonData6;
		}
		WriteJsonDataToFile(jsonData);
	}

	protected override void WriteJsonDataToFile(JsonData jsonData, string overrideFilePath = "")
	{
		base.WriteJsonDataToFile(jsonData, _tempRecoveryFilePath);
	}
}
