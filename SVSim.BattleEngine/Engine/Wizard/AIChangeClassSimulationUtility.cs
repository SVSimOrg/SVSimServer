using System.Collections.Generic;

namespace Wizard;

public class AIChangeClassSimulationUtility
{
	public static void ChangeClassAll(List<AIVirtualCard> candidates, CardBasePrm.ClanType classType)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.ChangeClass(classType);
			}
		}
	}

	public static void ChangeClassTarget(List<AIVirtualCard> candidates, CardBasePrm.ClanType classType, AIScriptTokenArgType whichTarget, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			ChangeClassAll(situationTarget.Targets, classType);
		}
	}
}
