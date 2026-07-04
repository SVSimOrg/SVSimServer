using LitJson;

namespace Wizard;

public class PracticeData
{
	public int ID { get; private set; }

	public string Text { get; private set; }

	public int ClassId { get; private set; }

	public int CharaId { get; private set; }

	public int DegreeId { get; private set; }

	public int AIDeckLevel { get; private set; }

	public int AILogicLevel { get; private set; }

	public int AIMaxLife { get; private set; }

	public int Battle3dFieldId { get; private set; }

	public bool IsMaintenance { get; private set; }

	public bool IsCampaign { get; private set; }

	public PracticeData(JsonData data)
	{
		ID = data["practice_id"].ToInt();
		Text = Data.Master.GetPracticeText(data["text_id"].ToString());
		ClassId = data["class_id"].ToInt();
		CharaId = data["chara_id"].ToInt();
		DegreeId = data["degree_id"].ToInt();
		AIDeckLevel = data["ai_deck_level"].ToInt();
		AILogicLevel = data["ai_logic_level"].ToInt();
		AIMaxLife = data["ai_max_life"].ToInt();
		IsCampaign = data["is_campaign_practice"].ToBoolean();
		if (IsCampaign)
		{
			Text = Data.SystemText.Get("Mission_0080") + Text;
		}
		int.TryParse(data["battle3dfield_id"].ToString(), out var result);
		Battle3dFieldId = result;
		IsMaintenance = data["is_maintenance"] != null && data["is_maintenance"].ToBoolean();
		if (IsMaintenance)
		{
			Text = Text + "  " + Data.Master.GetPracticeText("Practice_Maintenance");
		}
	}

	public PracticeData(int id, string text, int classId, int charaId, int degreeId, int aiDeckLevel, int aiLogicLevel, int aiMaxlife)
	{
		ID = id;
		Text = text;
		ClassId = classId;
		CharaId = charaId;
		DegreeId = degreeId;
		AIDeckLevel = aiDeckLevel;
		AILogicLevel = aiLogicLevel;
		AIMaxLife = aiMaxlife;
		Battle3dFieldId = 1;
		IsMaintenance = false;
	}
}
