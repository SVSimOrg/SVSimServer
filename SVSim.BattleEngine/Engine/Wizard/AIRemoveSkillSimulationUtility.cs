using System.Collections.Generic;

namespace Wizard;

public static class AIRemoveSkillSimulationUtility
{
	public static void RemoveSkillAll(List<AIVirtualCard> candidates, AISituationInfo situation)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			candidates[i].RemoveAllSkills(situation);
		}
	}

	public static void RemoveSkillTargetSelect(AIScriptTokenArgType whichTarget, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			RemoveSkillAll(situationTarget.Targets, situation);
		}
	}

	public static void RemoveSkillRandom(List<AIVirtualCard> candidates, AISituationInfo situation)
	{
		SelectRemoveSkillTarget(candidates, AISelectTargetPattern.Worst)?.RemoveAllSkills(situation);
	}

	private static AIVirtualCard SelectRemoveSkillTarget(List<AIVirtualCard> candidates, AISelectTargetPattern worstOrBest)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		float posiOrNega = ((worstOrBest == AISelectTargetPattern.Best) ? 1f : (-1f));
		float num = 0f;
		AIVirtualCard aIVirtualCard = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = candidates[i];
			if (aIVirtualCard == null)
			{
				num = CalcValue(aIVirtualCard2);
				aIVirtualCard = aIVirtualCard2;
				continue;
			}
			float num2 = CalcValue(aIVirtualCard2);
			if (num2 > num)
			{
				num = num2;
				aIVirtualCard = aIVirtualCard2;
			}
		}
		return aIVirtualCard;
		float CalcValue(AIVirtualCard card)
		{
			return (card.Value - (float)card.Attack - (float)card.Life) * posiOrNega;
		}
	}
}
