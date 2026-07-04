using System.Collections.Generic;

namespace Wizard;

public static class AISkillActivateCountUtility
{

	public static void AllActivateCountHolderIncrement(this AIVirtualField field, AISituationInfo situation, AIPlayTagType counterType, AIVirtualCard triggerCard = null)
	{
		if (field.CardListSet.HasActivateCountHolder)
		{
			List<AIVirtualCard> activateCountHolders = field.CardListSet.ActivateCountHolders;
			for (int i = 0; i < activateCountHolders.Count; i++)
			{
				activateCountHolders[i].Increment(situation, counterType, triggerCard);
			}
		}
	}

	public static void AllActivateCountHolderIncrement(this AIVirtualField field, AISituationInfo situation, AIPlayTagType counterType, List<AIVirtualCard> triggerCardList)
	{
		if (field.CardListSet.HasActivateCountHolder)
		{
			List<AIVirtualCard> activateCountHolders = field.CardListSet.ActivateCountHolders;
			for (int i = 0; i < activateCountHolders.Count; i++)
			{
				activateCountHolders[i].Increment(situation, counterType, triggerCardList);
			}
		}
	}

	public static void Increment(this AIVirtualCard card, AISituationInfo situation, AIPlayTagType counterType, AIVirtualCard triggerCard = null)
	{
		if (card.TagCollectionContainer.HasTag(counterType) && CheckCounterTypeCommonIncrementCondition(counterType, card, situation))
		{
			card.TagCollectionContainer.ActivateCountTags.Increment(card, situation, counterType, triggerCard);
		}
	}

	public static void Increment(this AIVirtualCard card, AISituationInfo situation, AIPlayTagType counterType, List<AIVirtualCard> triggerCardList)
	{
		if (card.TagCollectionContainer.HasTag(counterType) && CheckCounterTypeCommonIncrementCondition(counterType, card, situation))
		{
			card.TagCollectionContainer.ActivateCountTags.Increment(card, situation, counterType, triggerCardList);
		}
	}

	private static bool CheckCounterTypeCommonIncrementCondition(AIPlayTagType counterType, AIVirtualCard holder, AISituationInfo situation)
	{
		if (counterType == AIPlayTagType.TurnEndActivateCount)
		{
			return holder.IsAlly == situation.Actor.IsAlly;
		}
		return true;
	}

	public static int GetSkillActivateCount(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		if (!field.CardListSet.HasActivateCountHolder)
		{
			return 0;
		}
		if (!CheckActivateCounterFilteringArgument(argList, out var filters, out var skillOwnerId, out var skillIndex))
		{
			return 0;
		}
		int num = 0;
		List<AIVirtualCard> activateCountHolders = field.CardListSet.ActivateCountHolders;
		for (int i = 0; i < activateCountHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = activateCountHolders[i];
			if (AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, null, filters, playPtn, tagOwner, situation))
			{
				num += aIVirtualCard.GetSkillActivateCountFromOneCard(skillOwnerId, skillIndex);
			}
		}
		return num;
	}

	public static bool IsSkillOccurred(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		if (!field.CardListSet.HasActivateCountHolder)
		{
			return false;
		}
		if (!CheckActivateCounterFilteringArgument(argList, out var filters, out var skillOwnerId, out var skillIndex))
		{
			return false;
		}
		List<AIVirtualCard> activateCountHolders = field.CardListSet.ActivateCountHolders;
		for (int i = 0; i < activateCountHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = activateCountHolders[i];
			if (AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, null, filters, playPtn, tagOwner, situation) && !aIVirtualCard.GetIsSkillOccurredFromOneCard(skillOwnerId, skillIndex))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckActivateCounterFilteringArgument(List<AIScriptTokenBase> argList, out List<AIScriptTokenBase> filters, out int skillOwnerId, out int skillIndex)
	{
		filters = null;
		skillOwnerId = -1;
		skillIndex = 0;
		if (!(argList[0] is AIScriptNumericToken aIScriptNumericToken))
		{
			return false;
		}
		skillIndex = (int)aIScriptNumericToken.Value;
		argList.RemoveAt(0);
		if (!(argList[0] is AIScriptIDToken aIScriptIDToken))
		{
			return false;
		}
		skillOwnerId = aIScriptIDToken.ID;
		argList.RemoveAt(0);
		argList.Reverse();
		filters = argList;
		return true;
	}

	private static int GetSkillActivateCountFromOneCard(this AIVirtualCard counterOwner, int counterSourceCardId, int counterIndex)
	{
		return counterOwner.TagCollectionContainer.ActivateCountTags.GetActivateCount(counterSourceCardId, counterIndex);
	}

	private static bool GetIsSkillOccurredFromOneCard(this AIVirtualCard counterOwner, int counterSourceCardId, int counterIndex)
	{
		return counterOwner.TagCollectionContainer.ActivateCountTags.IsSkillOccurred(counterSourceCardId, counterIndex);
	}
}
