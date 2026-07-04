using System.Collections.Generic;

namespace Wizard;

public class AIDelayTurnEndTimePolicyCollection : AIPolicyCollection
{
	public float GetDelayTime(AIVirtualCard ownerCard, List<int> playPtn)
	{
		if (!base.HasPolicy)
		{
			return 0f;
		}
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.Argument is AIDelayTurnEndTime && aIPolicyData.CheckCondition(ownerCard, playPtn, ownerCard.SelfField, null))
			{
				return (aIPolicyData.Argument as AIDelayTurnEndTime).GetDelayTime(ownerCard, playPtn);
			}
		}
		return 0f;
	}
}
