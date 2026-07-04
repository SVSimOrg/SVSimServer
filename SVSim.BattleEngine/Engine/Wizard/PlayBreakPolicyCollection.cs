using System.Collections.Generic;

namespace Wizard;

public class PlayBreakPolicyCollection : AIPolicyCollection
{
	public bool IsPlayBreak(AIVirtualCard card, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasPolicy || !card.IsInHand)
		{
			return false;
		}
		AIVirtualField selfField = card.SelfField;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(card, playPtn, selfField, situation) && (aIPolicyData.Argument as AIPlayBreak).IsPlayBreak(card, selfField, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}
}
