using System.Collections.Generic;

namespace Wizard;

public static class AISpellboostSimulationUtility
{
	public static void SpellboostAll(List<AIVirtualCard> targets, int count)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].Spellboost(count);
		}
	}

	public static void SpellboostTarget(AISituationInfo situation, int count, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				aIVirtualCard.Spellboost(count);
			}
		}
	}

	public static void SpellboostWhenPlaySpellAtEvaluation(AIVirtualCard playCard, AIVirtualField field)
	{
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[i];
			if (aIVirtualCard.HasSpellboost)
			{
				aIVirtualCard.Spellboost(1);
			}
		}
	}

	public static void SpellboostWhenPlaySpell(AIVirtualCard playCard, AIVirtualField field)
	{
		if (!playCard.IsAlly)
		{
			return;
		}
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[i];
			if (!aIVirtualCard.IsSameCard(playCard) && aIVirtualCard.HasSpellboost)
			{
				aIVirtualCard.Spellboost(1);
			}
		}
	}

	public static AIVirtualCard SelectBestTargetForSpellboost(AIVirtualField field, int boostCount, List<AIVirtualCard> candidates)
	{
		AIVirtualCard result = null;
		float num = float.MinValue;
		List<int> list = new List<int>();
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			int spellboostCount = aIVirtualCard.SpellboostCount;
			list.Clear();
			list.Add(field.AllyHandCards.IndexOf(aIVirtualCard));
			float num2 = CalculateCardValueForSpellboostEvaluation(aIVirtualCard, list);
			aIVirtualCard.Spellboost(boostCount);
			float num3 = CalculateCardValueForSpellboostEvaluation(aIVirtualCard, list);
			aIVirtualCard.SetSpellboostCount(spellboostCount);
			float num4 = num3 - num2;
			if (num4 > num)
			{
				result = aIVirtualCard;
				num = num4;
			}
		}
		return result;
	}

	private static float CalculateCardValueForSpellboostEvaluation(AIVirtualCard card, List<int> playPtn)
	{
		return card.EvaluatePlayValue(playPtn) + card.GetHandBonus(playPtn, null, isIgnoreInFusion: false);
	}
}
