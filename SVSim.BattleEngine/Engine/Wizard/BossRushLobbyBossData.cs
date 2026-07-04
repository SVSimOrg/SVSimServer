using LitJson;

namespace Wizard;

public class BossRushLobbyBossData
{
	public enum BattleStatus
	{
		NO_BATTLE,
		WIN,
		CURRENT_BATTLE,
		LOSE
	}

	public BattleStatus Status { get; }

	public string Name { get; }

	public CardBasePrm.ClanType Class { get; }

	public int CharacterId { get; }

	public int Life { get; }

	public long EmblemId { get; }

	public long DegreeId { get; }

	public int AI { get; }

	public int QuestStageId { get; }

	public int Battle3dFieldId { get; }

	public string Skill { get; }

	public string SkillDescText { get; }

	public string BgmId { get; }

	public int RecoveryPoint { get; }

	public BossRushLobbyBossData(JsonData json, int index, int progress, bool isLose)
	{
		Name = json["name"].ToString();
		Class = (CardBasePrm.ClanType)json["enemy_class"].ToInt();
		CharacterId = json["enemy_chara_id"].ToInt();
		EmblemId = json["enemy_emblem_id"].ToLong();
		DegreeId = json["enemy_degree_id"].ToLong();
		AI = json["enemy_ai_id"].ToInt();
		QuestStageId = json["bossrush_stage_id"].ToInt();
		Battle3dFieldId = json["battle3dfield_id"].ToInt();
		BgmId = json["bgm_id"].ToString();
		Life = json["enemy_life"].ToInt();
		Skill = json["enemy_skill"].ToString();
		SkillDescText = json["enemy_skill_desc"].ToString();
		bool num = json["is_clear_battle"].ToBoolean();
		RecoveryPoint = json["recovery_point"].ToInt();
		if (num)
		{
			Status = BattleStatus.WIN;
		}
		else if (index == progress)
		{
			Status = (isLose ? BattleStatus.LOSE : BattleStatus.CURRENT_BATTLE);
		}
		else
		{
			Status = BattleStatus.NO_BATTLE;
		}
	}
}
