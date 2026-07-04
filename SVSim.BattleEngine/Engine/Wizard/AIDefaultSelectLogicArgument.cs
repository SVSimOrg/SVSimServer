using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIDefaultSelectLogicArgument : AISelectLogicArgumentBase
{
	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.DEFAULT_LOGIC;

	public AIDefaultSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		return candidates[0];
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		int num = Mathf.Min(candidates.Count, selectCount);
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < num; i++)
		{
			list.Add(candidates[i]);
		}
		return list;
	}
}
