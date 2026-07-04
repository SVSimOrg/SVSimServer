using System.Collections.Generic;

namespace Wizard;

public class PlayptnBonusPolicyCollection : AIPolicyCollection
{
	public float GetPlayptnBonus(AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasPolicy)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualCard allyClass = field.AllyClass;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(allyClass, playPtn, field, null))
			{
				num += aIPolicyData.EvalArg(allyClass, playPtn, field);
			}
		}
		return num;
	}
}
