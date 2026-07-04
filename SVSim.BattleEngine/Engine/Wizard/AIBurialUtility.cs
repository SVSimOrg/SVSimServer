using System.Collections.Generic;

namespace Wizard;

public static class AIBurialUtility
{
	public static bool IsBurialRite(AISituationInfo situation, List<AIScriptTokenBase> filters, AIVirtualField field)
	{
		AISelectedTargetInfo burialRiteTarget = situation.GetBurialRiteTarget();
		if (burialRiteTarget == null || burialRiteTarget.Type != TargetSelectType.BurialRite || !burialRiteTarget.HasTarget)
		{
			return false;
		}
		List<AIVirtualCard> targets = burialRiteTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			if (AIFilteringUtility.CheckMatchTargetFiltering(targets[i], field.AllyHandCards, filters, null, situation.Actor, situation))
			{
				return true;
			}
		}
		return false;
	}
}
