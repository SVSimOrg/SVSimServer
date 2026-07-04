using System.Collections.Generic;

namespace Wizard;

public static class AIHealCountUtility
{
	public static int GetHealCount(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, List<AIScriptTokenBase> argList)
	{
		if (argList.Count < 2)
		{
			return 0;
		}
		AIScriptTokenArgType argumentType = ((AIScriptArgumentToken)argList[0]).ArgumentType;
		AIScriptTokenArgType durationType;
		AIScriptTokenArgType aIScriptTokenArgType;
		if (argumentType == AIScriptTokenArgType.TURN || argumentType == AIScriptTokenArgType.GAME)
		{
			durationType = argumentType;
			aIScriptTokenArgType = AIScriptTokenArgType.NONE;
			argList.RemoveAt(0);
		}
		else
		{
			aIScriptTokenArgType = argumentType;
			durationType = ((AIScriptArgumentToken)argList[1]).ArgumentType;
			argList.RemoveRange(0, 2);
		}
		argList.Reverse();
		if (aIScriptTokenArgType == AIScriptTokenArgType.NONE)
		{
			return AIHealSimulationUtility.GetSelfTurnHealCountAll(owner, field, argList, playPtn, situation, durationType);
		}
		return AIHealSimulationUtility.GetSelfTurnHealCountAtCountType(owner, field, argList, playPtn, situation, durationType, aIScriptTokenArgType);
	}
}
