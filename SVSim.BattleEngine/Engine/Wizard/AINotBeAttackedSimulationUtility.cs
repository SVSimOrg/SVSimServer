using System.Collections.Generic;

namespace Wizard;

public static class AINotBeAttackedSimulationUtility
{
	public static void GiveNotBeAttackedToAll(List<AIVirtualCard> targets)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.NotBeAttacked();
			}
		}
	}

	public static void GiveNotBeAttackedToTargeted(AISituationInfo situation, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("GiveNotBeAttackedToTargeted error! No target!");
		}
		else
		{
			GiveNotBeAttackedToAll(situationTarget.Targets);
		}
	}
}
