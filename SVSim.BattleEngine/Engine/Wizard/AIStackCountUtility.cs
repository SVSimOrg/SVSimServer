using System.Collections.Generic;

namespace Wizard;

public static class AIStackCountUtility
{
	public static int GetStackCount(AIVirtualField field, bool isAlly)
	{
		List<AIVirtualCard> list = (isAlly ? field.AllyInplayCards : field.EnemyInplayCards);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].IsDead)
			{
				num += list[i].WhiteRitualCount;
			}
		}
		return num;
	}
}
