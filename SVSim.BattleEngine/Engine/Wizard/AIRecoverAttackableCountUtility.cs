using System.Collections.Generic;

namespace Wizard;

public static class AIRecoverAttackableCountUtility
{
	public static void RecoverAttackableCountTarget(AIScriptTokenArgType targetSelectType, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(targetSelectType))
		{
			RecoverAttackableCountAll(situation.GetSituationTarget(targetSelectType).Targets);
		}
	}

	public static void RecoverAttackableCountTargetPrediction(List<AIVirtualCard> candidatis, int selectCount)
	{
		if (selectCount > 1)
		{
			List<AIVirtualCard> list = SelectMultipleTarget(candidatis, selectCount, AISelectTargetPattern.Best);
			if (list != null && list.Count > 0)
			{
				RecoverAttackableCountAll(list);
			}
		}
		else
		{
			SelectSingleTarget(candidatis, AISelectTargetPattern.Best)?.RecoverAttackableCount();
		}
	}

	public static void RecoverAttackableCountAll(List<AIVirtualCard> targets)
	{
		if (targets != null)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].RecoverAttackableCount();
			}
		}
	}

	public static AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AISelectTargetPattern worstOrBest)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		AIVirtualCard result = null;
		int num = ((worstOrBest == AISelectTargetPattern.Best) ? int.MinValue : int.MaxValue);
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsUnit && aIVirtualCard.AttackableCount <= 0 && !aIVirtualCard.IsCantAttackAll)
			{
				int num2 = aIVirtualCard.Attack * aIVirtualCard.MaxAttackableCount;
				bool flag = false;
				switch (worstOrBest)
				{
				case AISelectTargetPattern.Best:
					flag = num2 > num;
					break;
				case AISelectTargetPattern.Worst:
					flag = num2 < num;
					break;
				}
				if (flag)
				{
					result = aIVirtualCard;
					num = num2;
				}
			}
		}
		return result;
	}

	private static List<AIVirtualCard> SelectMultipleTarget(List<AIVirtualCard> candidates, int selectCount, AISelectTargetPattern worstOrBest)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list = null;
		for (int i = 0; i < selectCount; i++)
		{
			if (list != null && list.Count == selectCount)
			{
				break;
			}
			AIVirtualCard aIVirtualCard = null;
			int num = ((worstOrBest == AISelectTargetPattern.Best) ? int.MinValue : int.MaxValue);
			for (int j = 0; j < candidates.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = candidates[j];
				if (aIVirtualCard2.IsUnit && aIVirtualCard2.AttackableCount <= 0 && !aIVirtualCard2.IsCantAttackAll && (list == null || !list.Contains(aIVirtualCard2)))
				{
					int num2 = aIVirtualCard2.Attack * aIVirtualCard2.MaxAttackableCount;
					bool flag = false;
					switch (worstOrBest)
					{
					case AISelectTargetPattern.Best:
						flag = num2 > num;
						break;
					case AISelectTargetPattern.Worst:
						flag = num2 < num;
						break;
					}
					if (flag)
					{
						aIVirtualCard = aIVirtualCard2;
						num = num2;
					}
				}
			}
			if (aIVirtualCard != null)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		return list;
	}
}
