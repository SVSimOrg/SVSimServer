using LitJson;
using Wizard.Battle.Recovery;

namespace Wizard;

public class QuestBattleData
{
	public int QuestStageId { get; private set; }

	public bool IsExtra { get; private set; }

	public bool IsMockBattle { get; private set; }

	public int ExtraDeckScheduleId { get; private set; }

	public int Battle3dFieldID { get; private set; }

	public string BgmId { get; private set; }

	public int CharaId { get; private set; }

	public int EnemyClass { get; private set; }

	public int EnemyAiId { get; private set; }

	public int EmblemId { get; private set; }

	public int DegreeId { get; private set; }

	public int PlayerEmotionOverride { get; private set; }

	public int EnemyEmotionOverride { get; private set; }

	public QuestBattleData(JsonData data)
	{
		QuestStageId = data["quest_stage_id"].ToInt();
		IsExtra = data["is_extra"].ToInt() == 1;
		IsMockBattle = data["is_mock_battle"].ToBoolean();
		ExtraDeckScheduleId = data["extra_deck_schedule_id"].ToInt();
		Battle3dFieldID = data["battle3dfield_id"].ToInt();
		string text = data["bgm_id"].ToString();
		BgmId = (string.IsNullOrEmpty(text) ? "NONE" : text);
		CharaId = data["texture_id"].ToInt();
		EnemyClass = data["enemy_class"].ToInt();
		EnemyAiId = data["enemy_ai_id"].ToInt();
		EmblemId = data["enemy_emblem_id"].ToInt();
		DegreeId = data["enemy_degree_id"].ToInt();
		PlayerEmotionOverride = data["player_emotion_override"].ToInt();
		EnemyEmotionOverride = data["enemy_emotion_override"].ToInt();
	}

	public QuestBattleData(SetupConditionInfo setupInfo)
	{
		QuestStageId = setupInfo.QuestStageId;
		IsExtra = setupInfo.QuestIsExtra;
		IsMockBattle = setupInfo.QuestIsMockBattle;
		ExtraDeckScheduleId = setupInfo.QuestExtraDeckScheduleId;
		Battle3dFieldID = setupInfo.BackGroundId;
		BgmId = setupInfo.BgmId;
		CharaId = setupInfo.EnemyInfo.CharaId;
		EnemyClass = setupInfo.EnemyInfo.ClassId;
		EnemyAiId = setupInfo.QuestEnemyAiId;
		EmblemId = setupInfo.QuestEnemyEmblemId;
		DegreeId = setupInfo.QuestEnemyDegreeId;
		PlayerEmotionOverride = setupInfo.QuestPlayerEmotionOverride;
		EnemyEmotionOverride = setupInfo.QuestEnemyEmotionOverride;
	}
}
