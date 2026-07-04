using System.Collections.Generic;

namespace Wizard;

public static class AIChangeCostSimulationUtility
{
	public static void AddCostAll(int count, List<AIVirtualCard> targets)
	{
		if (targets != null && targets.Count > 0)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].AddCurrentCost(count);
			}
		}
	}

	public static void AddCostTarget(AIVirtualCard owner, int count, List<AIVirtualCard> candidates, List<AIVirtualCard> targets)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!owner.IsSameCard(aIVirtualCard) && candidates.Contains(aIVirtualCard))
			{
				aIVirtualCard.AddCurrentCost(count);
			}
		}
	}

	public static void SetCostAll(int count, List<AIVirtualCard> targets)
	{
		if (targets != null && targets.Count > 0)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].SetCurrentCost(count);
			}
		}
	}

	public static void SetCostTarget(AIVirtualCard owner, int count, List<AIVirtualCard> candidates, List<AIVirtualCard> targets)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!owner.IsSameCard(aIVirtualCard) && candidates.Contains(aIVirtualCard))
			{
				aIVirtualCard.SetCurrentCost(count);
			}
		}
	}

	public static AIVirtualCard SelectTargetForChangeCost(AIVirtualCard owner, List<AIVirtualCard> candidates, AISelectTargetPattern worstOrBest)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		AIVirtualCard aIVirtualCard = candidates[0];
		switch (worstOrBest)
		{
		case AISelectTargetPattern.Best:
		{
			for (int j = 1; j < candidates.Count; j++)
			{
				AIVirtualCard aIVirtualCard3 = candidates[j];
				if (!owner.IsSameCard(aIVirtualCard3) && aIVirtualCard.Cost < aIVirtualCard3.Cost)
				{
					aIVirtualCard = aIVirtualCard3;
				}
			}
			break;
		}
		case AISelectTargetPattern.Worst:
		{
			for (int i = 1; i < candidates.Count; i++)
			{
				AIVirtualCard aIVirtualCard2 = candidates[i];
				if (!owner.IsSameCard(aIVirtualCard2) && aIVirtualCard.Cost > aIVirtualCard2.Cost)
				{
					aIVirtualCard = aIVirtualCard2;
				}
			}
			break;
		}
		}
		return aIVirtualCard;
	}

	public static void ExecuteCostChange(AIScriptTokenArgType changeType, AIScriptTokenArgType whichTarget, int changeCost, AIVirtualCard owner, List<AIVirtualCard> candidates, AISituationInfo situation, AIVirtualField field)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		switch (whichTarget)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			ChangeCostValue(isTargetAll: true, changeType, changeCost, owner, candidates, null);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (situation != null && situation.IsTargetExists(whichTarget))
			{
				AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
				if (situationTarget == null || !situationTarget.HasTarget)
				{
					AIConsoleUtility.Log("ExecuteCostChange Nothing TargetInfo");
					break;
				}
				List<AIVirtualCard> targets2 = situationTarget.Targets;
				ChangeCostValue(isTargetAll: false, changeType, changeCost, owner, candidates, targets2);
			}
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			AIVirtualCard aIVirtualCard = SelectTargetForChangeCost(owner, candidates, AISelectTargetPattern.Worst);
			if (aIVirtualCard != null)
			{
				List<AIVirtualCard> targets = new List<AIVirtualCard> { aIVirtualCard };
				ChangeCostValue(isTargetAll: false, changeType, changeCost, owner, candidates, targets);
			}
			break;
		}
		default:
			AIConsoleUtility.LogError("ExecuteCostChange: ilegal Target Arg type [" + whichTarget.ToString() + "]");
			break;
		}
	}

	public static void ChangeCostValue(bool isTargetAll, AIScriptTokenArgType changeType, int changeCost, AIVirtualCard owner, List<AIVirtualCard> candidates, List<AIVirtualCard> targets)
	{
		switch (changeType)
		{
		case AIScriptTokenArgType.ADD:
			if (isTargetAll)
			{
				AddCostAll(changeCost, candidates);
			}
			else
			{
				AddCostTarget(owner, changeCost, candidates, targets);
			}
			break;
		case AIScriptTokenArgType.SET:
			if (isTargetAll)
			{
				SetCostAll(changeCost, candidates);
			}
			else
			{
				SetCostTarget(owner, changeCost, candidates, targets);
			}
			break;
		default:
			AIConsoleUtility.LogError("ChangeCostValue: ilegal Cost Arg type [" + changeType.ToString() + "]");
			break;
		}
	}
}
