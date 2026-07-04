using System.Collections.Generic;

namespace Wizard;

public class PlayerEmoOnTurnEndPolicyCollection : AIPolicyCollection
{
	public int GetPlayerEmoOnTurnEnd(AIVirtualField field, bool isAllyTurn)
	{
		if (base.HasPolicy)
		{
			AIVirtualCard enemyClass = field.EnemyClass;
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			for (int i = 0; i < base.PolicyList.Count; i++)
			{
				AIPolicyData aIPolicyData = base.PolicyList[i];
				if (aIPolicyData.CheckCondition(enemyClass, emptyPlayPtn, field, null))
				{
					int emoteIdIfSideIsCorrect = (aIPolicyData.Argument as AIEmoteOnTurnTransition).GetEmoteIdIfSideIsCorrect(isAllyTurn);
					if (emoteIdIfSideIsCorrect >= 0)
					{
						return emoteIdIfSideIsCorrect;
					}
				}
			}
		}
		return -1;
	}
}
