using System.Collections.Generic;

namespace Wizard;

public class DisableLethalCheckPolicyCollection : AIPolicyCollection
{
	public bool IsDisableLethalCheck(AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasPolicy)
		{
			return false;
		}
		AIVirtualCard allyClass = field.AllyClass;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			if (base.PolicyList[i].CheckCondition(allyClass, playPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}
}
