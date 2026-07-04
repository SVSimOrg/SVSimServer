using System.IO;

namespace Wizard.Battle.Recovery;

public class SingleBattleRecoveryRecordManager : RecoveryRecordManagerBase
{

	protected override string DefaultRecoveryFileName => "recovery_single.json";

	public SingleBattleRecoveryRecordManager()
	{
	}

	public SingleBattleRecoveryRecordManager(string filePath)
		: base(filePath)
	{
	}

	public override void SetupRecording(BattleManagerBase battleMgr, DataMgr.BattleType battleType, int randomSeed, int backGroundId, string bgmId = "NONE")
	{
		base.SetupRecording(battleMgr, battleType, randomSeed, backGroundId, bgmId);
		RecordSingleBattleSettings(_recorder, battleType, randomSeed, backGroundId, bgmId);
	}

	protected override OperationRecorderBase CreateOperationRecorder()
	{
		if (RecoveryRecordManagerBase.IsExistsSingleRecoveryFile())
		{
			string tempFilePath = OperationRecorderBase.RecordDirectoryPath + "temp_recovery_single.json";
			RecoveryRecordManagerBase.DeleteRecoveryFile();
			return new SingleBattleOperationRecorder(_recoveryFilePath, tempFilePath);
		}
		return new SingleBattleOperationRecorder(_recoveryFilePath);
	}

	protected override void SetupRecorderEvents(OperationRecorderBase operationRecorder, BattleManagerBase battleMgr)
	{
		base.SetupRecorderEvents(operationRecorder, battleMgr);
		battleMgr.OperateMgr.OnTurnEnd += operationRecorder.RecordTurnEnd;
	}

	protected void RecordSingleBattleSettings(OperationRecorderBase operationRecorder, DataMgr.BattleType battleType, int randomSeed, int backGroundId, string bgmId)
	{
		DataMgr dataMgr = _gameMgr.GetDataMgr();
		operationRecorder.RecordBattleType(battleType);
		operationRecorder.RecordRandomSeed(randomSeed);
		operationRecorder.RecordBackGroundId(backGroundId);
		operationRecorder.RecordBgmId(bgmId);
		operationRecorder.RecordClass("player", dataMgr.GetPlayerClassId());
		operationRecorder.RecordSubClass("player", dataMgr.GetPlayerSubClassId());
		if (dataMgr.TryGetPlayerMyRotationInfo(out var myRotationInfo))
		{
			operationRecorder.RecordMyRotationId("player", myRotationInfo.Id);
		}
		operationRecorder.RecordClass("enemy", dataMgr.GetEnemyClassId());
		operationRecorder.RecordSubClass("enemy", dataMgr.GetEnemySubClassId());
		if (dataMgr.TryGetEnemyMyRotationInfo(out var myRotationInfo2))
		{
			operationRecorder.RecordMyRotationId("enemy", myRotationInfo2.Id);
		}
		operationRecorder.RecordChara("player", dataMgr.GetPlayerCharaId());
		operationRecorder.RecordChara("enemy", dataMgr.GetEnemyCharaId());
		operationRecorder.RecordSleeve("player", dataMgr.GetPlayerSleeveId());
		operationRecorder.RecordSleeve("enemy", dataMgr.GetEnemySleeveId());
		operationRecorder.RecordDeck("player", 'p', dataMgr.GetCurrentDeckData());
		operationRecorder.RecordDeck("enemy", 'e', dataMgr.GetCurrentEnemyDeckData());
		operationRecorder.RecordEnemyAIDifficulty(dataMgr.m_EnemyAIDifficulty);
		operationRecorder.RecordEnemyAILogicLevel(dataMgr.m_EnemyAILogicLevel);
		operationRecorder.RecordEnemyAIMaxLife(dataMgr.m_EnemyAIMaxLife);
		operationRecorder.RecordEnemyAIDeckId(dataMgr.m_EnemyAIDeckId);
		operationRecorder.RecordEnemyAIStyleId(dataMgr.m_EnemyAIStyleId);
		operationRecorder.RecordEnemyAIEmoteId(dataMgr.m_EnemyAIEmoteId);
		operationRecorder.RecordEnemyAIUseInnerEmote(dataMgr.m_EnemyAIUseInnerEmote);
		operationRecorder.RecordPracticeDifficultyDegreeId(dataMgr.PracticeDifficultyDegreeId);
		operationRecorder.RecordIsPreBuildDeck(dataMgr.IsLastSelectDeckAttributeType(DeckAttributeType.BuildDeck));
		operationRecorder.RecordIsTrialDeck(dataMgr.IsLastSelectDeckAttributeType(DeckAttributeType.TrialDeck));
		operationRecorder.RecordIsDefaultDeck(dataMgr.GetSelectDefDeck());
		operationRecorder.RecordMissionNecessaryInformation(dataMgr.MissionNecessaryInformation);
		switch (battleType)
		{
		case DataMgr.BattleType.Quest:
			operationRecorder.RecordQuestStageId(dataMgr.QuestBattleData.QuestStageId);
			operationRecorder.RecordQuestEnemyAiId(dataMgr.QuestBattleData.EnemyAiId);
			operationRecorder.RecordQuestEnemyEmblemId(dataMgr.QuestBattleData.EmblemId);
			operationRecorder.RecordQuestEnemyDegreeId(dataMgr.QuestBattleData.DegreeId);
			operationRecorder.RecordQuestEnemyEmotionOverride(dataMgr.QuestBattleData.EnemyEmotionOverride);
			operationRecorder.RecordQuestPlayerEmotionOverride(dataMgr.QuestBattleData.PlayerEmotionOverride);
			operationRecorder.RecordQuestIsExtra(dataMgr.QuestBattleData.IsExtra);
			operationRecorder.RecordQuestIsMockBattle(dataMgr.QuestBattleData.IsMockBattle);
			operationRecorder.RecordQuestExtraDeckScheduleId(dataMgr.QuestBattleData.ExtraDeckScheduleId);
			break;
		case DataMgr.BattleType.BossRushQuest:
			operationRecorder.RecordQuestStageId(dataMgr.BossRushBattleData.QuestStageId);
			operationRecorder.RecordQuestEnemyAiId(dataMgr.BossRushBattleData.EnemyAiId);
			operationRecorder.RecordQuestEnemyEmblemId(dataMgr.BossRushBattleData.EmblemId);
			operationRecorder.RecordQuestEnemyDegreeId(dataMgr.BossRushBattleData.DegreeId);
			operationRecorder.RecordQuestRecoveryPoint(dataMgr.BossRushBattleData.RecoveryPointWhenFinish);
			operationRecorder.RecordQuestPlayerSkillList(dataMgr.BossRushBattleData.PlayerSkillList);
			operationRecorder.RecordQuestEnemySkill(dataMgr.BossRushBattleData.EnemySkill);
			operationRecorder.RecordQuestMaxBattleCount(dataMgr.BossRushBattleData.MaxBattleCount);
			operationRecorder.RecordQuestCurrentWinCount(dataMgr.BossRushBattleData.CurrentWinCount);
			break;
		case DataMgr.BattleType.SecretBossQuest:
			operationRecorder.RecordQuestStageId(dataMgr.BossRushBattleData.QuestStageId);
			operationRecorder.RecordQuestEnemyAiId(dataMgr.BossRushBattleData.EnemyAiId);
			operationRecorder.RecordQuestEnemyEmblemId(dataMgr.BossRushBattleData.EmblemId);
			operationRecorder.RecordQuestEnemyDegreeId(dataMgr.BossRushBattleData.DegreeId);
			operationRecorder.RecordQuestPlayerSkillList(dataMgr.BossRushBattleData.PlayerSkillList);
			operationRecorder.RecordQuestEnemySkill(dataMgr.BossRushBattleData.EnemySkill);
			break;
		case DataMgr.BattleType.Practice:
			operationRecorder.RecordPractice3DFieldId(dataMgr.GetSoroPlay3DFieldID());
			break;
		case DataMgr.BattleType.Story:
			operationRecorder.RecordStoryData();
			break;
		}
	}

	public void RecordChangeAI(string logicName, int operationQueueCount)
	{
		if (_recorder is SingleBattleOperationRecorder singleBattleOperationRecorder)
		{
			singleBattleOperationRecorder.RecordChangeAI(logicName, operationQueueCount);
		}
	}
}
