using System.Collections.Generic;

namespace Wizard;

public class AIMaxAttackSelectLogicArgument : AISelectLogicArgumentBase
{
	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.MAX_ATTACK_LOGIC;

	public AIMaxAttackSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIVirtualCard aIVirtualCard = null;
		int compare = 0;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = candidates[i];
			if (aIVirtualCard2.IsUnit && (aIVirtualCard == null || IsWellChosenTarget(aIVirtualCard2, compare, worstOrBest)))
			{
				aIVirtualCard = aIVirtualCard2;
				compare = aIVirtualCard2.Attack;
			}
		}
		return aIVirtualCard;
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		LogNotImplementMultipleSelect();
		return null;
	}

	private bool IsWellChosenTarget(AIVirtualCard target, int compare, AISelectTargetPattern worstOrBest)
	{
		return worstOrBest switch
		{
			AISelectTargetPattern.Best => target.Attack > compare, 
			AISelectTargetPattern.Worst => target.Attack < compare, 
			_ => false, 
		};
	}
}
