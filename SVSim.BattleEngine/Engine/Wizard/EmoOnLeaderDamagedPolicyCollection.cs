using System.Collections.Generic;

namespace Wizard;

public class EmoOnLeaderDamagedPolicyCollection : AIPolicyCollection
{
	public int GetEmoOnLeaderDamaged(AIVirtualField field)
	{
		if (!base.HasPolicy)
		{
			return -1;
		}
		AIVirtualCard allyClass = field.AllyClass;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(allyClass, emptyPlayPtn, field, null))
			{
				int num = (int)aIPolicyData.EvalArg(allyClass, emptyPlayPtn, field);
				if (num >= 0)
				{
					return num;
				}
			}
		}
		return -1;
	}
}
