using System.Collections.Generic;

namespace Wizard;

public class AIBounceSelectLogicArgument : AISelectLogicArgumentBase
{
	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.BOUNCE_LOGIC;

	public AIBounceSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		return AISimulationRemovalUtility.SelectRemovalTarget(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Bounce);
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		return AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, tagOwner, field, playPtn, situation, worstOrBest, AIRemovalType.Bounce, selectCount);
	}
}
