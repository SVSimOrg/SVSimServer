using System.Collections.Generic;

namespace Wizard;

public static class AIHandBuffSimulationUtility
{
	public static void ExecuteHandBuffAll(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsInHand && aIVirtualCard.IsUnit)
			{
				aIVirtualCard.GiveBuff(situation, buffInfo, isTemp: false);
				list.Add(aIVirtualCard);
			}
		}
		if (list.Count > 0)
		{
			situation.RegisterLatestTargetList(list);
		}
	}

	public static void ExecuteHandBuffRandom(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates != null && candidates.Count > 0)
		{
			AIVirtualCard aIVirtualCard = SelectHandBuffTarget(candidates, AISelectTargetPattern.Worst, playPtn, situation);
			if (aIVirtualCard != null)
			{
				aIVirtualCard.GiveBuff(situation, buffInfo, isTemp: false);
				situation.RegisterSingleLatestTarget(aIVirtualCard);
			}
		}
	}

	public static void ExecuteHandBuffTargetSelect(AIBuffExecutingInfo_old buffInfo, AIScriptTokenArgType whichTarget, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			ExecuteHandBuffAll(situationTarget.Targets, buffInfo, situation);
		}
	}

	public static void ExecuteHandBuffBestTarget(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo, AISituationInfo situation)
	{
		AIVirtualCard aIVirtualCard = SelectHandBuffTarget(candidates, AISelectTargetPattern.Best, EnemyAI.EmptyPlayPtn, situation);
		if (aIVirtualCard != null)
		{
			aIVirtualCard.GiveBuff(situation, buffInfo, isTemp: false);
			situation.RegisterSingleLatestTarget(aIVirtualCard);
		}
	}

	public static AIVirtualCard SelectHandBuffTarget(List<AIVirtualCard> candidates, AISelectTargetPattern worstOrBest, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		float num = 0f;
		AIVirtualCard aIVirtualCard = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = candidates[i];
			float handBonus = aIVirtualCard2.GetHandBonus(playPtn, situation, isIgnoreInFusion: false);
			if (aIVirtualCard == null)
			{
				aIVirtualCard = aIVirtualCard2;
				num = handBonus;
				continue;
			}
			switch (worstOrBest)
			{
			case AISelectTargetPattern.Best:
				if (handBonus > num)
				{
					num = handBonus;
					aIVirtualCard = aIVirtualCard2;
				}
				else
				{
					if (handBonus < num)
					{
						break;
					}
					if (aIVirtualCard2.Cost < aIVirtualCard.Cost)
					{
						aIVirtualCard = aIVirtualCard2;
					}
					else if (aIVirtualCard2.Cost <= aIVirtualCard.Cost)
					{
						if (aIVirtualCard2.Attack > aIVirtualCard.Attack)
						{
							aIVirtualCard = aIVirtualCard2;
						}
						else if (aIVirtualCard2.Attack == aIVirtualCard.Attack && aIVirtualCard2.Life > aIVirtualCard.Life)
						{
							aIVirtualCard = aIVirtualCard2;
						}
					}
				}
				break;
			case AISelectTargetPattern.Worst:
				if (handBonus < num)
				{
					num = handBonus;
					aIVirtualCard = aIVirtualCard2;
				}
				else
				{
					if (handBonus > num)
					{
						break;
					}
					if (aIVirtualCard2.Cost > aIVirtualCard.Cost)
					{
						aIVirtualCard = aIVirtualCard2;
					}
					else if (aIVirtualCard2.Cost <= aIVirtualCard.Cost)
					{
						if (aIVirtualCard2.Attack < aIVirtualCard.Attack)
						{
							aIVirtualCard = aIVirtualCard2;
						}
						else if (aIVirtualCard2.Attack == aIVirtualCard.Attack && aIVirtualCard2.Life < aIVirtualCard.Life)
						{
							aIVirtualCard = aIVirtualCard2;
						}
					}
				}
				break;
			}
		}
		return aIVirtualCard;
	}
}
