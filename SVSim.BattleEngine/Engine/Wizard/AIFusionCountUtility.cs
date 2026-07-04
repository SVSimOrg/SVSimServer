using System.Collections.Generic;

namespace Wizard;

public static class AIFusionCountUtility
{
	public static int GetFusionCount(AIVirtualCard tagOwner, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> allFusionIngredientsList = tagOwner.GetAllFusionIngredientsList(situation);
		if (allFusionIngredientsList == null || allFusionIngredientsList.Count <= 0)
		{
			return 0;
		}
		argList.Reverse();
		return AIFilteringUtility.MultipleFiltering(allFusionIngredientsList, argList, tagOwner, playPtn, situation)?.Count ?? 0;
	}

	public static int GetFusionCountAtOnce(AIVirtualCard tagOwner, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || !situation.Actor.IsSameCard(tagOwner))
		{
			return 0;
		}
		List<AIVirtualCard> list = null;
		if (situation is AIFusionSituationInfo || situation is AIVirtualTargetSelectAction { ActionType: AIOperationType.FUSION })
		{
			if (situation.IsTargetExists(AIScriptTokenArgType.TARGET_SELECT))
			{
				argList.Reverse();
				list = AIFilteringUtility.MultipleFiltering(situation.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT).Targets, argList, tagOwner, playPtn, situation);
			}
		}
		else
		{
			AIConsoleUtility.LogError("AIFusionCountUtility.GetFusionCountAtOnce(): Unexpected situation type. Not fusion situation.");
		}
		return list?.Count ?? 0;
	}

	public static int GetNowFusionCount(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn)
	{
		if (tagOwner.FusionIngredients == null)
		{
			return 0;
		}
		return AIFilteringUtility.MultipleFiltering(tagOwner.FusionIngredients.CardList, filters, tagOwner, playPtn, null)?.Count ?? 0;
	}

	public static int GetFusionNameCount(AIVirtualCard tagOwner, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> allFusionIngredientsList = tagOwner.GetAllFusionIngredientsList(situation);
		if (allFusionIngredientsList == null || allFusionIngredientsList.Count <= 0)
		{
			return 0;
		}
		argList.Reverse();
		return AIFilteringUtility.GetCardNameCountFromList(allFusionIngredientsList, argList, tagOwner, playPtn, situation);
	}

	public static int GetFusedCardCountInGame(AIVirtualCard tagOwner, List<AIScriptTokenBase> filter, List<int> playPtn, AISituationInfo situation, bool isWithoutSameId)
	{
		List<AIVirtualCard> list = (tagOwner.IsAlly ? tagOwner.SelfField.AllyGameFusedCards : tagOwner.SelfField.EnemyGameFusedCards);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(list, filter, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list2 == null)
		{
			return 0;
		}
		int count = list2.Count;
		if (isWithoutSameId)
		{
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < list2.Count; i++)
			{
				hashSet.Add(list2[i].BaseId);
			}
			count = hashSet.Count;
		}
		return count;
	}

	private static List<AIVirtualCard> GetAllFusionIngredientsList(this AIVirtualCard card, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		if (card.FusionIngredients != null && card.FusionIngredients.HasFusionIngredients)
		{
			list = new List<AIVirtualCard>(card.FusionIngredients.CardList);
		}
		else if (card.BeforeTransformCard != null && card.BeforeTransformCard.FusionIngredients != null && card.BeforeTransformCard.FusionIngredients.HasFusionIngredients)
		{
			list = new List<AIVirtualCard>(card.BeforeTransformCard.FusionIngredients.CardList);
		}
		if (situation != null && situation is AIFusionSituationInfo aIFusionSituationInfo && aIFusionSituationInfo.Actor.IsSameCard(card) && aIFusionSituationInfo.IsTargetExists(AIScriptTokenArgType.TARGET_SELECT))
		{
			list = AIParamQuery.AddRangeToList(aIFusionSituationInfo.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT).Targets, list);
		}
		return list;
	}
}
