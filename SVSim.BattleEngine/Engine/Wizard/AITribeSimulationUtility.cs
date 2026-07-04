using System.Collections.Generic;

namespace Wizard;

public static class AITribeSimulationUtility
{
	public static CardBasePrm.TribeType ConvertTokenArgTypeToTribeType(AIScriptTokenArgType tokenArgType)
	{
		return tokenArgType switch
		{
			AIScriptTokenArgType.FOOD => CardBasePrm.TribeType.FOOD, 
			AIScriptTokenArgType.MACHINE => CardBasePrm.TribeType.MACHINE, 
			AIScriptTokenArgType.NATURE => CardBasePrm.TribeType.NATURE, 
			AIScriptTokenArgType.LEGION => CardBasePrm.TribeType.LEGION, 
			AIScriptTokenArgType.LORD => CardBasePrm.TribeType.LORD, 
			AIScriptTokenArgType.LEVIN => CardBasePrm.TribeType.LEVIN, 
			AIScriptTokenArgType.LOOT => CardBasePrm.TribeType.LOOTING, 
			AIScriptTokenArgType.WHITE_RITUAL => CardBasePrm.TribeType.WHITE_RITUAL, 
			AIScriptTokenArgType.MANARIA => CardBasePrm.TribeType.MANARIA, 
			AIScriptTokenArgType.ARTIFACT => CardBasePrm.TribeType.ARTIFACT, 
			AIScriptTokenArgType.BANQUET => CardBasePrm.TribeType.BANQUET, 
			AIScriptTokenArgType.HERO => CardBasePrm.TribeType.HERO, 
			AIScriptTokenArgType.ARMED => CardBasePrm.TribeType.ARMED, 
			AIScriptTokenArgType.HELLBOUND => CardBasePrm.TribeType.HELLBOUND, 
			AIScriptTokenArgType.SCHOOL => CardBasePrm.TribeType.SCHOOL, 
			AIScriptTokenArgType.CHESS => CardBasePrm.TribeType.CHESS, 
			_ => CardBasePrm.TribeType.MAX, 
		};
	}

	public static void ChangeTribeAll(List<AIVirtualCard> candidates, CardBasePrm.TribeType tribe)
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
				aIVirtualCard.ChangeTribe(tribe);
			}
		}
	}

	public static void ChangeTribeTargetSelect(CardBasePrm.TribeType tribe, AIScriptTokenArgType selectType, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(selectType);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			ChangeTribeAll(situationTarget.Targets, tribe);
		}
	}
}
