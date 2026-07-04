using System.Collections.Generic;

namespace Wizard;

public class PlayerEmoOnLeaderDamagedPolicyCollection : AIPolicyCollection
{
	public int GetPlayerEmoOnLeaderDamaged(AIVirtualField field)
	{
		if (!base.HasPolicy)
		{
			return -1;
		}
		AIVirtualCard enemyClass = field.EnemyClass;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(enemyClass, emptyPlayPtn, field, null))
			{
				int num = (int)aIPolicyData.EvalArg(enemyClass, emptyPlayPtn, field);
				if (num >= 0)
				{
					return num;
				}
			}
		}
		return -1;
	}
}
