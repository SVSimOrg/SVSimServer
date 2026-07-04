using System.Collections.Generic;

namespace Wizard;

public class AIMetamorphoseSelectLogicArgument : AISelectLogicArgumentBase
{

	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.METAMORPHOSE_LOGIC;

	public AIMetamorphoseSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIRemovalEvaluationOption aIRemovalEvaluationOption = CreateMetamorphoseOption(tagOwner, field);
		if (aIRemovalEvaluationOption == null)
		{
			return null;
		}
		return AISimulationRemovalUtility.SelectRemovalTarget(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Metamorphose, aIRemovalEvaluationOption);
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIRemovalEvaluationOption aIRemovalEvaluationOption = CreateMetamorphoseOption(tagOwner, field);
		if (aIRemovalEvaluationOption == null)
		{
			return null;
		}
		return AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Metamorphose, selectCount, aIRemovalEvaluationOption);
	}

	private AIRemovalEvaluationOption CreateMetamorphoseOption(AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (_argumentList == null || _argumentList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIMetamorphoseSelectLogicArgument error!! _argumentList is null");
			return null;
		}
		int num = _argumentList[0].EvalID();
		if (num == -1)
		{
			AIConsoleUtility.LogError("AIMetamorphoseSelectLogicArgument error!! tokenId is invalid");
			return null;
		}
		return AIMetamorphoseSimulationUtility.CreateMetamorphoseEvaluationOption(tagOwner, field, num);
	}
}
