using LitJson;

namespace Wizard;

public class BossRushSpecialSkill
{
	public int OriginalCardId;

	public string Name;

	public string SkillText;

	public string SkillDescText;

	public bool IsFoil { get; set; }

	public BossRushSpecialSkill()
	{
	}

	public BossRushSpecialSkill(int cardId, string name, string skillText, string skillDescText, bool isFoil)
	{
		OriginalCardId = cardId;
		Name = name;
		SkillText = skillText;
		SkillDescText = skillDescText;
		IsFoil = isFoil;
	}

	public BossRushSpecialSkill(JsonData jsonData)
	{
		OriginalCardId = jsonData["original_card_id"].ToInt();
		Name = jsonData["name"].ToString();
		SkillText = jsonData["skill_text"].ToString();
		SkillDescText = jsonData["skill_desc_text"].ToString();
		IsFoil = jsonData["is_foil"].ToBoolean();
	}
}
