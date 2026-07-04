using System.Collections.Generic;
using Wizard.Battle.UI;

namespace Wizard;

public static class AIBanAttackSimulationUtility
{
	public static void BanAttackAll(List<AIVirtualCard> candidates, CantAttackType banAttackType)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (!aIVirtualCard.IsIndependent)
			{
				BanAttackSingle(aIVirtualCard, banAttackType);
			}
		}
	}

	public static void BanAttackRandom(List<AIVirtualCard> candidates, CantAttackType banAttackType, int selectCount)
	{
		if (selectCount <= 1)
		{
			AIVirtualCard aIVirtualCard = SelectTargetForBanAttack(candidates, AISelectTargetPattern.Worst);
			if (aIVirtualCard != null && !aIVirtualCard.IsIndependent)
			{
				BanAttackSingle(aIVirtualCard, banAttackType);
			}
		}
		else
		{
			BanAttackAll(SelectMultipleTargetsForBanAttack(candidates, selectCount, AISelectTargetPattern.Worst), banAttackType);
		}
	}

	public static void BanAttackTarget(AISituationInfo situation, CantAttackType banAttackType, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			BanAttackAll(situationTarget.Targets, banAttackType);
		}
	}

	public static void BanAttackTargetPrediction(List<AIVirtualCard> candidates, CantAttackType banAttackType, int selectCount)
	{
		if (selectCount <= 1)
		{
			AIVirtualCard aIVirtualCard = SelectTargetForBanAttack(candidates, AISelectTargetPattern.Best);
			if (aIVirtualCard != null && !aIVirtualCard.IsIndependent)
			{
				BanAttackSingle(aIVirtualCard, banAttackType);
			}
		}
		else
		{
			BanAttackAll(SelectMultipleTargetsForBanAttack(candidates, selectCount, AISelectTargetPattern.Best), banAttackType);
		}
	}

	private static void BanAttackSingle(AIVirtualCard target, CantAttackType banAttackType)
	{
		switch (banAttackType)
		{
		case CantAttackType.All:
			target.IsCantAttackAll = true;
			break;
		case CantAttackType.Unit:
			target.IsSkillCantAttackUnit = true;
			break;
		case CantAttackType.Class:
			target.IsSkillCantAttackClass = true;
			break;
		case CantAttackType.NotHasGuard:
			target.IsSkillCantAtkUnitNotHasGuard = true;
			break;
		}
	}

	private static AIVirtualCard SelectTargetForBanAttack(List<AIVirtualCard> candidates, AISelectTargetPattern worstOrBest)
	{
		int num = ((worstOrBest == AISelectTargetPattern.Worst) ? int.MaxValue : int.MinValue);
		AIVirtualCard result = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if ((worstOrBest != AISelectTargetPattern.Worst) ? (aIVirtualCard.Attack > num) : (aIVirtualCard.Attack < num))
			{
				result = aIVirtualCard;
				num = aIVirtualCard.Attack;
			}
		}
		return result;
	}

	private static List<AIVirtualCard> SelectMultipleTargetsForBanAttack(List<AIVirtualCard> candidates, int selectCount, AISelectTargetPattern worstOrBest)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = list[j];
				if ((worstOrBest != AISelectTargetPattern.Worst) ? (aIVirtualCard.Attack > aIVirtualCard2.Attack) : (aIVirtualCard.Attack < aIVirtualCard2.Attack))
				{
					list[j] = aIVirtualCard;
					aIVirtualCard = aIVirtualCard2;
					flag = true;
				}
			}
			if (!flag && list.Count < selectCount)
			{
				list.Add(aIVirtualCard);
			}
		}
		return list;
	}
}
