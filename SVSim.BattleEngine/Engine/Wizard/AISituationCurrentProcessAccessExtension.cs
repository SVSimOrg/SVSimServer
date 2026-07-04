using System.Collections.Generic;

namespace Wizard;

public static class AISituationCurrentProcessAccessExtension
{

	public static bool IsSameCurrentTriggerCardAndTriggerType(this AISituationInfo situation, AIVirtualCard card, AISituationTriggerInformation.TriggerType triggerType)
	{
		if (situation.IsNull())
		{
			return false;
		}
		AISituationTriggerInformation triggerInfo = situation.CurrentSkillProcessInfo.TriggerInfo;
		if (triggerInfo.TriggerCard == null || triggerInfo.Type != triggerType)
		{
			return false;
		}
		return triggerInfo.IsTriggerCardAndTriggerType(card, triggerType);
	}

	public static bool IsSameCurrentTriggerCard(this AISituationInfo situation, AIVirtualCard card)
	{
		if (situation.IsNull())
		{
			return false;
		}
		AISituationTriggerInformation triggerInfo = situation.CurrentSkillProcessInfo.TriggerInfo;
		if (triggerInfo.TriggerCard == null)
		{
			return false;
		}
		return triggerInfo.IsTriggerCard(card);
	}

	public static bool IsOwnSummonedCard(this AISituationInfo situation, AIVirtualCard card, AIScriptTokenArgType targetCardType, bool isLatest = false)
	{
		if (situation.IsNull())
		{
			return false;
		}
		List<AIVirtualCard> list = (isLatest ? situation.GetOwnLatestSummonedCards() : situation.GetOwnSummonedCards());
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		switch (targetCardType)
		{
		case AIScriptTokenArgType.FOLLOWER:
			if (!card.IsUnit)
			{
				return false;
			}
			break;
		case AIScriptTokenArgType.AMULET:
			if (!card.IsAmulet)
			{
				return false;
			}
			break;
		default:
			AIConsoleUtility.LogError($"IsOwnSummonedCard(): Unsupported target card type. type:{targetCardType}");
			return false;
		case AIScriptTokenArgType.ALL:
			break;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsLatestOwnDrewCard(this AISituationInfo situation, AIVirtualCard card)
	{
		if (situation.IsNull())
		{
			return false;
		}
		List<AIVirtualCard> ownLatestDrewCards = situation.GetOwnLatestDrewCards();
		if (ownLatestDrewCards == null || ownLatestDrewCards.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < ownLatestDrewCards.Count; i++)
		{
			if (ownLatestDrewCards[i].IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsLatestTarget(this AISituationInfo situation, AIVirtualCard card)
	{
		List<AIVirtualCard> latestTargets = situation.GetLatestTargets();
		if (latestTargets == null || latestTargets.Count <= 0)
		{
			AIConsoleUtility.LogError("IsLatestTarget error!! candidates is empty!!!!!");
			return false;
		}
		for (int i = 0; i < latestTargets.Count; i++)
		{
			if (card.IsSameCard(latestTargets[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static List<AIVirtualCard> GetOwnDestroyedCards(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return null;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.OwnDestroyedCards;
	}

	public static List<AIVirtualCard> GetOwnSummonedCards(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return null;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.OwnSummonedCards;
	}

	public static List<AIVirtualCard> GetOwnLatestSummonedCards(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return null;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.OwnLatestSummonedCards;
	}

	public static List<AIVirtualCard> GetOwnLatestDrewCards(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return null;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.OwnLatestDrewCards;
	}

	public static List<AIVirtualCard> GetLatestTargets(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return null;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.LatestTargets;
	}

	public static int GetCurrentProcessDefaultDamage(this AISituationInfo situation)
	{
		if (situation.IsNull())
		{
			return -1;
		}
		return situation.CurrentSkillProcessInfo.OwnProcessRecord.DefaultDamage;
	}

	public static void RegisterOwnDestroyedCard(this AISituationInfo situation, AIVirtualCard card)
	{
		if (!situation.IsNull())
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.AddOwnDestroyedCard(card);
		}
	}

	public static void RegisterOwnBanishedCard(this AISituationInfo situation, AIVirtualCard card)
	{
		if (!situation.IsNull())
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.AddOwnBanishedCard(card);
		}
	}

	public static void RegisterOwnSummonedCardList(this AISituationInfo situation, List<AIVirtualCard> cards)
	{
		if (!situation.IsNull() && cards != null && cards.Count > 0)
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.AddOwnSummonedCards(cards);
		}
	}

	public static void RegisterOwnDrewCardList(this AISituationInfo situation, List<AIVirtualCard> cards)
	{
		if (!situation.IsNull() && cards != null && cards.Count > 0)
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.AddOwnDrewCards(cards);
		}
	}

	public static void RegisterSingleLatestTarget(this AISituationInfo situation, AIVirtualCard card)
	{
		if (!situation.IsNull())
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.RegisterSingleLatestTarget(card);
		}
	}

	public static void RegisterLatestTargetList(this AISituationInfo situation, List<AIVirtualCard> list)
	{
		if (!situation.IsNull() && list != null && list.Count > 0)
		{
			situation.CurrentSkillProcessInfo.OwnProcessRecord.RegisterLatestTargetList(list);
		}
	}

	private static bool IsNull(this AISituationInfo situation)
	{
		if (situation != null)
		{
			return situation.CurrentSkillProcessInfo == null;
		}
		return true;
	}
}
