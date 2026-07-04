using System.Collections.Generic;
using System.IO;
using Cute;
using LitJson;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Recovery;

public abstract class OperationRecorderBase
{
	protected readonly string _filePath;

	protected readonly List<JsonData> _operationJsonDataList = new List<JsonData>();

	protected readonly List<string> _skillTargetList = new List<string>();

	public static string RecordDirectoryName => "recovery";

	public static string RecordDirectoryPath => Application.persistentDataPath + "/" + RecordDirectoryName + "/";

	public OperationRecorderBase(string filePath)
	{
		_filePath = filePath;
	}

	public abstract void RecordBattleType(DataMgr.BattleType battleType);

	public abstract void RecordRandomSeed(int randomSeed);

	public abstract void RecordBackGroundId(int backGroundId);

	public abstract void RecordBgmId(string bgmId);

	public abstract void RecordClass(string playerName, int clanType);

	public abstract void RecordSubClass(string playerName, int clanType);

	public abstract void RecordMyRotationId(string playerName, string myRotationId);

	public abstract void RecordSleeve(string playerName, long sleeveId);

	public abstract void RecordChara(string playerName, int charaId);

	public abstract void RecordDeck(string playerName, char indexHeadChar, IEnumerable<int> cardIds);

	public abstract void RecordEnemyAIDifficulty(int difficulty);

	public abstract void RecordEnemyAILogicLevel(int level);

	public abstract void RecordEnemyAIMaxLife(int life);

	public abstract void RecordEnemyAIDeckId(int deckId);

	public abstract void RecordEnemyAIStyleId(int styleId);

	public abstract void RecordEnemyAIEmoteId(int emoteId);

	public abstract void RecordEnemyAIUseInnerEmote(bool useInnerEmote);

	public abstract void RecordStartTurnIsPlayer(bool startTurnIsPlayer);

	public abstract void RecordPracticeDifficultyDegreeId(int degreeId);

	public abstract void RecordIsPreBuildDeck(bool isPreBuildDeck);

	public abstract void RecordIsTrialDeck(bool isTrialDeck);

	public abstract void RecordIsDefaultDeck(bool isDefaultDeck);

	public abstract void RecordQuestStageId(int id);

	public abstract void RecordQuestEnemyAiId(int id);

	public abstract void RecordQuestEnemyEmblemId(int id);

	public abstract void RecordQuestEnemyDegreeId(int id);

	public abstract void RecordQuestEnemyEmotionOverride(int id);

	public abstract void RecordQuestPlayerEmotionOverride(int id);

	public abstract void RecordQuestRecoveryPoint(int recoveryPoint);

	public abstract void RecordQuestPlayerSkillList(List<BossRushSpecialSkill> skills);

	public abstract void RecordQuestEnemySkill(BossRushSpecialSkill skill);

	public abstract void RecordQuestMaxBattleCount(int maxBattleCount);

	public abstract void RecordQuestCurrentWinCount(int currentWinCount);

	public abstract void RecordQuestIsExtra(bool isExtra);

	public abstract void RecordQuestIsMockBattle(bool isMockBattle);

	public abstract void RecordQuestExtraDeckScheduleId(int id);

	public abstract void RecordMissionNecessaryInformation(BattleManagerBase.MissionNecessaryInformation missionNecessaryInformation);

	public virtual void RecordStoryData()
	{
	}

	public abstract void RecordPractice3DFieldId(int id);

	public abstract void RecordMulliganStart();

	public abstract void RecordPlayerMulliganReplaceCards(IEnumerable<BattleCardBase> replaceCards, IEnumerable<int> completeCards);

	public abstract void RecordEnemyMulliganReplaceCards(IEnumerable<BattleCardBase> replaceCards, IEnumerable<int> completeCards);

	public abstract void RecordPlay(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCard);

	public abstract VfxBase RecordAttack(BattleCardBase attackCard, BattleCardBase targetCard, SkillProcessor skillProcessor);

	public abstract void RecordEvolve(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCard);

	public abstract void RecordTurnStart();

	public abstract void RecordTurnEnd();

	public abstract void RecordRetire();

	public abstract void RecordSkillTargets(IEnumerable<BattleCardBase> targetCards);

	public abstract void RecordStartFusion(BattleCardBase fusionCard, List<BattleCardBase> selectableCards);

	public abstract void RecordSelectFusion(BattleCardBase card);

	public abstract void RecordCompleteFusionSelect(BattleCardBase fusionCard, IEnumerable<BattleCardBase> ingredientCards);

	public abstract void RecordCancelFusion(BattleCardBase card);

	public abstract void RecordStartSelect(BattleCardBase card, bool isEvolve, List<BattleCardBase> selectableCard, bool isChoiceBrave);

	public abstract void RecordSelect(BattleCardBase card, bool isEvolve, BattleCardBase actCard, bool isBurialRite, bool isChoiceBrave);

	public abstract void RecordCompleteSelect(BattleCardBase card, bool isEvolve, BattleCardBase actCard, bool isChoiceBrave, bool isBurialRite);

	public abstract void RecordStartChoice(BattleCardBase card, bool isEvolve, List<BattleCardBase> choiceCards, bool isChoiceBrave);

	public abstract void RecordCompleteChoice(BattleCardBase card, bool isEvolve, List<BattleCardBase> chosenCardList, BattleCardBase actCard, List<int> chosenCardIndexList, bool hasSelectionSkill, bool isChoiceBrave);

	public abstract void RecordCancelChoice(BattleCardBase card, bool isEvolve, bool isChoiceBrave);

	public abstract void RecordCancelSelect(BattleCardBase actCard, bool isEvolve, bool isChoiceBrave);

	protected abstract void WriteJsonData();

	protected virtual void WriteJsonDataToFile(JsonData jsonData, string overrideFilePath = "")
	{
		string text = (string.IsNullOrEmpty(overrideFilePath) ? _filePath : overrideFilePath);
		try
		{
			using StreamWriter streamWriter = new StreamWriter(text);
			streamWriter.Write(CryptAES.encryptForNode(jsonData.ToJson()));
		}
		catch
		{
			LocalLog.AccumulateTraceLog("File Unauthorized to Access:" + text);
		}
	}

	protected string CardToIndexName(IBattleCardUniqueID card)
	{
		if (card == null)
		{
			return string.Empty;
		}
		return card.GetName();
	}

	protected void WriteSkillTargetInfoToJson(IEnumerable<BattleCardBase> selectedCards, JsonData jsonData)
	{
		if (!selectedCards.IsNotNullOrEmpty())
		{
			return;
		}
		JsonData jsonData2 = new JsonData();
		jsonData2.SetJsonType(JsonType.Array);
		foreach (BattleCardBase selectedCard in selectedCards)
		{
			jsonData2.Add(CardToIndexName(selectedCard));
		}
		jsonData["skill_target"] = jsonData2;
	}
}
