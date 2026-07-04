using System.Collections.Generic;

namespace Wizard;

public class EpValuePolicyCollection : AIPolicyCollection
{
	public float GetEpValue(AISituationInfo situation, List<int> playPtn)
	{
		if (!base.HasPolicy)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualCard actor = situation.Actor;
		AIVirtualField selfField = actor.SelfField;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(actor, playPtn, selfField, situation))
			{
				num += aIPolicyData.EvalArg(actor, playPtn, selfField, situation);
			}
		}
		return num;
	}
}
