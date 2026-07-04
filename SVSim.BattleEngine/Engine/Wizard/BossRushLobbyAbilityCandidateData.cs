using LitJson;

namespace Wizard;

public class BossRushLobbyAbilityCandidateData
{
	public int AbilityId { get; }

	public string AbilityDetail { get; }

	public bool IsFoil { get; }

	public string CardName { get; }

	public BossRushLobbyAbilityCandidateData(JsonData json)
	{
		AbilityId = json["ability_id"].ToInt();
		AbilityDetail = ((json["special_ability_desc"] == null) ? string.Empty : json["special_ability_desc"].ToString());
		IsFoil = json["is_foil"].ToBoolean();
		CardName = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId(AbilityId).CardName;
	}
}
