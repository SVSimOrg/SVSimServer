using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MyRotationInfo
{
	public class MyRotationBonus
	{
		public string AbilityId { get; }

		public string[] AttachAbilities { get; }

		public int AddStartPp { get; }

		public int AddStartLife { get; }

		public int IncreaseAddPptotalTurn { get; }

		public int IncreaseAddPptotalAmount { get; }

		public string AbilityDesc { get; }

		public bool IsEpRecoverySkill { get; }

		public string IconName { get; }

		public string DarkIconName { get; }

		private string OriginalAbilityText { get; }

		public MyRotationBonus(string abilityId, string abilityText, int addStartPp, int addStartLife, int increaseAddPptotalTurn, int increaseAddPptotalAmount, string abilityDesc)
		{
			AbilityId = abilityId;
			OriginalAbilityText = abilityText;
			AttachAbilities = abilityText.Split(',');
			AddStartPp = addStartPp;
			AddStartLife = addStartLife;
			IncreaseAddPptotalTurn = increaseAddPptotalTurn;
			IncreaseAddPptotalAmount = increaseAddPptotalAmount;
			AbilityDesc = abilityDesc;
			for (int i = 0; i < AttachAbilities.Length; i++)
			{
				if (AttachAbilities[i].Contains("possess_ep_modifier"))
				{
					IsEpRecoverySkill = true;
				}
			}
			IconName = "my_rotation_ability_icon_" + abilityId;
			DarkIconName = "my_rotation_ability_icon_dark_" + abilityId;
		}
	}

	private List<UnlimitedRestrictedCard> _restrictedCardData = new List<UnlimitedRestrictedCard>();

	private List<int> _rePrintCardList = new List<int>();

	public string Id { get; }

	public List<string> CardPadkIdList { get; }

	public string PackSelectText { get; private set; }

	public string LastPackText { get; private set; }

	public List<string> AbilityIdList { get; private set; }

	public List<MyRotationBonus> Abilities { get; private set; }

	public Dictionary<int, MyRotationRePrintInfo> RePrintDictionary { get; private set; } = new Dictionary<int, MyRotationRePrintInfo>();

	public bool IsEnableNemesis { get; private set; }

	public MyRotationInfo(JsonData json)
	{
		Id = json["rotation_id"].ToString();
		CardPadkIdList = new List<string>(json["card_set_ids"].ToString().Split('|'));
		AbilityIdList = new List<string>(json["abilities"].ToString().Split('|'));
	}

	public bool IsRePrintCard(int baseCardId)
	{
		return _rePrintCardList.Contains(baseCardId);
	}

	public bool IsRePrintCardAvailablePack(int baseCardId, string cardPackId)
	{
		if (!RePrintDictionary.TryGetValue(baseCardId, out var value))
		{
			return false;
		}
		return value.IsRePrintCardAvailablePack(cardPackId);
	}

	public int GetSameCardCount(int baseCardId)
	{
		foreach (UnlimitedRestrictedCard restrictedCardDatum in _restrictedCardData)
		{
			if (baseCardId == restrictedCardDatum.BaseCardId)
			{
				return restrictedCardDatum.Count;
			}
		}
		return FormatBehaviorManager.GetDefaultBehaviour(Format.MyRotation).DeckSameKindCardNumMax;
	}

	public bool IsNotUseCard(int baseCardId)
	{
		return GetSameCardCount(baseCardId) == 0;
	}

	public bool IsEnableCardPackId(string cardPackId)
	{
		foreach (string cardPadkId in CardPadkIdList)
		{
			if (cardPadkId == cardPackId)
			{
				return true;
			}
		}
		return false;
	}
}
