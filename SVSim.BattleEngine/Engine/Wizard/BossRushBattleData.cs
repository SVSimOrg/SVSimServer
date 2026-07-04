using System.Collections.Generic;
using Wizard.Battle.Recovery;

namespace Wizard;

public class BossRushBattleData
{
	public int QuestStageId { get; private set; }

	public int Battle3dFieldID { get; private set; }

	public string BgmId { get; private set; }

	public int CharaId { get; private set; }

	public int EnemyAiId { get; private set; }

	public int EmblemId { get; private set; }

	public int DegreeId { get; private set; }

	public int PlayerEmotionOverride { get; private set; }

	public int EnemyEmotionOverride { get; private set; }

	public bool IsMockBattle { get; private set; }

	public int RecoveryPointWhenFinish { get; private set; }

	public List<BossRushSpecialSkill> PlayerSkillList { get; private set; }

	public BossRushSpecialSkill EnemySkill { get; private set; }

	public int MaxBattleCount { get; }

	public int CurrentWinCount { get; }

	public BossRushBattleData(BossRushLobbyBossData bossData, List<BossRushSpecialSkill> playerSkillList, BossRushSpecialSkill enemySkill, BossRushLobbyData lobbyData)
	{
		QuestStageId = bossData.QuestStageId;
		Battle3dFieldID = bossData.Battle3dFieldId;
		BgmId = bossData.BgmId;
		CharaId = bossData.CharacterId;
		EnemyAiId = bossData.AI;
		EmblemId = (int)bossData.EmblemId;
		DegreeId = (int)bossData.DegreeId;
		RecoveryPointWhenFinish = bossData.RecoveryPoint;
		PlayerSkillList = playerSkillList;
		EnemySkill = enemySkill;
		MaxBattleCount = lobbyData.BossDataList.Count;
		CurrentWinCount = lobbyData.WinCount;
	}

	public BossRushBattleData(QuestBossData bossData, List<BossRushSpecialSkill> playerSkillList, BossRushSpecialSkill enemySkill)
	{
		QuestStageId = bossData.StageId;
		Battle3dFieldID = bossData.Battle3dFieldId;
		BgmId = bossData.BgmId;
		CharaId = bossData.CharacterId;
		EnemyAiId = bossData.AI;
		EmblemId = (int)bossData.EmblemId;
		DegreeId = (int)bossData.DegreeId;
		RecoveryPointWhenFinish = bossData.RecoveryPoint;
		PlayerSkillList = playerSkillList;
		EnemySkill = enemySkill;
	}

	public BossRushBattleData(SetupConditionInfo setupInfo)
	{
		QuestStageId = setupInfo.QuestStageId;
		Battle3dFieldID = setupInfo.BackGroundId;
		BgmId = setupInfo.BgmId;
		CharaId = setupInfo.EnemyInfo.CharaId;
		EnemyAiId = setupInfo.QuestEnemyAiId;
		EmblemId = setupInfo.QuestEnemyEmblemId;
		DegreeId = setupInfo.QuestEnemyDegreeId;
		RecoveryPointWhenFinish = setupInfo.RecoveryPoint;
		PlayerSkillList = setupInfo.QuestPlayerSkillList;
		EnemySkill = setupInfo.QuestEnemySkill;
		MaxBattleCount = setupInfo.QuestMaxBattleCount;
		CurrentWinCount = setupInfo.QuestCurrentWinCount;
	}
}
