using System.Collections.Generic;

namespace Wizard;

public class EmoOnTurnEndPolicyCollection : AIPolicyCollection
{
	public int GetEmoOnTurnEnd(AIVirtualField field, bool isAllyTurn)
	{
		if (base.HasPolicy)
		{
			AIVirtualCard allyClass = field.AllyClass;
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			for (int i = 0; i < base.PolicyList.Count; i++)
			{
				AIPolicyData aIPolicyData = base.PolicyList[i];
				if (aIPolicyData.CheckCondition(allyClass, emptyPlayPtn, field, null))
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
