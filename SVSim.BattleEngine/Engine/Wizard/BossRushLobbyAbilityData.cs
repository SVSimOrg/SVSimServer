using LitJson;

namespace Wizard;

public class BossRushLobbyAbilityData
{
	public string Name { get; }

	public bool IsFoil { get; }

	public string Skill { get; }

	public string SkillDescText { get; }

	public int DisplayCardId { get; }

	public BossRushLobbyAbilityData(JsonData json)
	{
		int cardId = json["ability_id"].ToInt();
		IsFoil = json["is_foil"].ToBoolean();
		CardParameter cardParameterFromId = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId(cardId);
		DisplayCardId = cardId;
		Name = Data.SystemText.Get("BossRush_0011", cardParameterFromId.CardName);
		Skill = json["skill"].ToString();
		SkillDescText = json["special_ability_desc"].ToString();
	}
}
