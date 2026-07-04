using System.Collections.Generic;

namespace Wizard;

public class InplayMovePatternFilter
{
	public bool IsSupposedToBeBestBreakPattern { get; private set; }

	public bool IsBestBreakPattern { get; private set; }

	public InplayMovePatternFilter()
	{
		IsSupposedToBeBestBreakPattern = false;
		IsBestBreakPattern = false;
	}

	public void CheckBreakPatternCondition(AIVirtualField field, List<int> enemyTargets)
	{
		IsSupposedToBeBestBreakPattern = false;
		IsBestBreakPattern = false;
		bool flag = false;
		for (int i = 0; i < field.EnemyInplayCards.Count; i++)
		{
			bool flag2 = enemyTargets.Contains(i);
			AIVirtualCard aIVirtualCard = field.EnemyInplayCards[i];
			if ((flag2 && aIVirtualCard.Value <= 0f) || (!flag2 && aIVirtualCard.Value > 0f))
			{
				flag = true;
				break;
			}
		}
		IsSupposedToBeBestBreakPattern = !flag;
	}

	public void ConfirmBestPattern(List<AIVirtualCard> enemyTargets)
	{
		if (IsBestBreakPattern || !IsSupposedToBeBestBreakPattern)
		{
			return;
		}
		for (int i = 0; i < enemyTargets.Count; i++)
		{
			if (!enemyTargets[i].IsDead)
			{
				return;
			}
		}
		IsBestBreakPattern = true;
	}
}
