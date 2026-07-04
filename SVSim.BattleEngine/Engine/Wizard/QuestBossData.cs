using LitJson;

namespace Wizard;

public class QuestBossData
{
	public string Name { get; set; }

	public CardBasePrm.ClanType Class { get; set; }

	public int CharacterId { get; set; }

	public int Life { get; set; }

	public long EmblemId { get; set; }

	public long DegreeId { get; set; }

	public int AI { get; set; }

	public int StageId { get; set; }

	public int Battle3dFieldId { get; set; }

	public string BgmId { get; set; }

	public string Skill { get; set; }

	public string SkillDescText { get; set; }

	public int RecoveryPoint { get; set; }

	public QuestBossData(JsonData json)
	{
		Name = json["name"].ToString();
		Class = (CardBasePrm.ClanType)json["enemy_class"].ToInt();
		CharacterId = json["enemy_chara_id"].ToInt();
		EmblemId = json["enemy_emblem_id"].ToLong();
		DegreeId = json["enemy_degree_id"].ToLong();
		AI = json["enemy_ai_id"].ToInt();
		Life = json["enemy_life"].ToInt();
		Battle3dFieldId = json["battle3dfield_id"].ToInt();
		BgmId = json["bgm_id"].ToString();
		StageId = (json.Keys.Contains("quest_stage_id") ? json["quest_stage_id"].ToInt() : json["bossrush_stage_id"].ToInt());
		Skill = (json.Keys.Contains("enemy_skill") ? json["enemy_skill"].ToString() : string.Empty);
		SkillDescText = (json.Keys.Contains("enemy_skill_desc") ? json["enemy_skill_desc"].ToString() : string.Empty);
		RecoveryPoint = (json.Keys.Contains("recovery_point") ? json["recovery_point"].ToInt() : (-1));
	}
}
