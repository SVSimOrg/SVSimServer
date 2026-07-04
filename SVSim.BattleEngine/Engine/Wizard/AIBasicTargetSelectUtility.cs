using System;
using System.Collections.Generic;

namespace Wizard;

public static class AIBasicTargetSelectUtility
{
	public static AIVirtualCard SelectSingleBasicSkillTarget(List<AIVirtualCard> candidates, AIScriptTokenArgType skillType, AISelectTargetPattern worstOrBest)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		Func<AIVirtualCard, AIVirtualCard, AISelectTargetPattern, AIVirtualCard> func = SelectCompareFunc(skillType);
		if (func == null)
		{
			return candidates[0];
		}
		AIVirtualCard aIVirtualCard = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = candidates[i];
			if (aIVirtualCard2.IsUnit && !aIVirtualCard2.IsDead)
			{
				aIVirtualCard = ((aIVirtualCard != null) ? func(aIVirtualCard, aIVirtualCard2, worstOrBest) : aIVirtualCard2);
			}
		}
		return aIVirtualCard;
	}

	private static Func<AIVirtualCard, AIVirtualCard, AISelectTargetPattern, AIVirtualCard> SelectCompareFunc(AIScriptTokenArgType skillType)
	{
		switch (skillType)
		{
		case AIScriptTokenArgType.SNEAK:
			return CompareSneakCandidates;
		case AIScriptTokenArgType.QUICK:
		case AIScriptTokenArgType.RUSH:
			return CompareRushOrQuickCandidates;
		case AIScriptTokenArgType.GUARD:
			return CompareGuardCandidates;
		case AIScriptTokenArgType.KILLER:
			return CompareKillerCandidates;
		case AIScriptTokenArgType.IGNORE_GUARD:
			return CompareIgnoreGuardCandidates;
		case AIScriptTokenArgType.DRAIN:
		case AIScriptTokenArgType.UNTOUCHABLE:
		case AIScriptTokenArgType.FORCE_TARGETING:
		case AIScriptTokenArgType.UNBANISHABLE:
			return null;
		default:
			return null;
		}
	}

	private static AIVirtualCard CompareRushOrQuickCandidates(AIVirtualCard currentResultTarget, AIVirtualCard checkCard, AISelectTargetPattern worstOrBest)
	{
		int attack = currentResultTarget.Attack;
		int attack2 = checkCard.Attack;
		AIVirtualCard result = currentResultTarget;
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
			if (checkCard.IsSummonDrunkenness && !checkCard.IsCantAttackAll && attack2 > attack)
			{
				result = checkCard;
			}
			break;
		case AISelectTargetPattern.Worst:
			if (attack2 < attack)
			{
				result = checkCard;
			}
			break;
		}
		return result;
	}

	private static AIVirtualCard CompareGuardCandidates(AIVirtualCard currentResultTarget, AIVirtualCard checkCard, AISelectTargetPattern worstOrBest)
	{
		int life = currentResultTarget.Life;
		int life2 = checkCard.Life;
		AIVirtualCard result = currentResultTarget;
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
			if (life2 > life)
			{
				result = checkCard;
			}
			break;
		case AISelectTargetPattern.Worst:
			if (life2 < life)
			{
				result = checkCard;
			}
			break;
		}
		return result;
	}

	private static AIVirtualCard CompareSneakCandidates(AIVirtualCard currentResultTarget, AIVirtualCard checkCard, AISelectTargetPattern worstOrBest)
	{
		int attack = currentResultTarget.Attack;
		int life = currentResultTarget.Life;
		int attack2 = checkCard.Attack;
		int life2 = checkCard.Life;
		AIVirtualCard result = currentResultTarget;
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
			if (attack2 > attack)
			{
				result = checkCard;
			}
			else if (attack2 == attack && life2 > life)
			{
				result = checkCard;
			}
			break;
		case AISelectTargetPattern.Worst:
			if (attack2 < attack)
			{
				result = checkCard;
			}
			else if (attack2 == attack && life2 < life)
			{
				result = checkCard;
			}
			break;
		}
		return result;
	}

	private static AIVirtualCard CompareKillerCandidates(AIVirtualCard currentResultTarget, AIVirtualCard checkCard, AISelectTargetPattern worstOrBest)
	{
		int attack = currentResultTarget.Attack;
		int attack2 = checkCard.Attack;
		AIVirtualCard result = currentResultTarget;
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
			if (attack > attack2 || (attack == attack2 && currentResultTarget.Life < checkCard.Life))
			{
				result = checkCard;
			}
			break;
		case AISelectTargetPattern.Worst:
			if (attack < attack2 || (attack == attack2 && currentResultTarget.Life > checkCard.Life))
			{
				result = checkCard;
			}
			break;
		}
		return result;
	}

	private static AIVirtualCard CompareIgnoreGuardCandidates(AIVirtualCard currentResultTarget, AIVirtualCard checkCard, AISelectTargetPattern worstOrBest)
	{
		int attack = currentResultTarget.Attack;
		int attack2 = checkCard.Attack;
		AIVirtualCard result = currentResultTarget;
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
			if (attack2 > attack)
			{
				result = checkCard;
			}
			break;
		case AISelectTargetPattern.Worst:
			if (attack2 < attack)
			{
				result = checkCard;
			}
			break;
		}
		return result;
	}
}
