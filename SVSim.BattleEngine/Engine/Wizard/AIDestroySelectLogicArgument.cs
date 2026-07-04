using System.Collections.Generic;

namespace Wizard;

public class AIDestroySelectLogicArgument : AISelectLogicArgumentBase
{
	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.DESTROY_LOGIC;

	public AIDestroySelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		return AISimulationRemovalUtility.SelectRemovalTarget(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Destroy);
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		return AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Destroy, selectCount);
	}
}
