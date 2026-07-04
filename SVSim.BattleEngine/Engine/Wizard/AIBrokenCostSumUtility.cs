using System.Collections.Generic;

namespace Wizard;

public class AIBrokenCostSumUtility
{
	public static int GetBrokenCostSum(AIVirtualField field, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		List<AIVirtualCard> list = (tagOwner.IsAlly ? field.CardListSet.AllyDestroyedCards : field.CardListSet.EnemyDestroyedCards);
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, list, filters, playPtn, tagOwner, situation))
			{
				num += aIVirtualCard.BaseCost;
			}
		}
		return num;
	}
}
