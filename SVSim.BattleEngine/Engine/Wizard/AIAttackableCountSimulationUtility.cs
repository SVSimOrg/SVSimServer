using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIAttackableCountSimulationUtility : MonoBehaviour
{
	public static void ExecuteChangeAttackableCountAll(List<AIVirtualCard> candidates, int attackableCount)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsDead)
			{
				aIVirtualCard.GiveAttackableCount(attackableCount);
			}
		}
	}

	public static void ExecuteChangeAttackableCountTargetSelect(int attackableCount, AIScriptTokenArgType whichTarget, AISituationInfo situation)
	{
		if (situation != null)
		{
			AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
			if (situationTarget != null && situationTarget.HasTarget)
			{
				ExecuteChangeAttackableCountAll(situationTarget.Targets, attackableCount);
			}
		}
	}
}
