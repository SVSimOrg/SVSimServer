using System.Collections.Generic;

namespace Wizard;

public class ModUnitRatePolicyCollection : AIPolicyCollection
{
	public float GetUnitRate(AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasPolicy)
		{
			return 1f;
		}
		float num = 1f;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(card, playPtn, field, null))
			{
				num *= aIPolicyData.EvalArg(card, playPtn, field);
			}
		}
		return num;
	}
}
