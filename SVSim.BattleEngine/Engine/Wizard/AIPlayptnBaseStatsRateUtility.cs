using System.Collections.Generic;

namespace Wizard;

public static class AIPlayptnBaseStatsRateUtility
{
	public static void Execute(List<AIVirtualCard> srcCards, List<AIVirtualCard> targetCards, List<int> playPtn)
	{
		for (int i = 0; i < srcCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = srcCards[i];
			if (!aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.PlayptnBaseStatsRate))
			{
				continue;
			}
			List<PlayptnBaseStatsRateInfo> executeInfo = aIVirtualCard.TagCollectionContainer.PlayptnBaseStatsRateTags.GetExecuteInfo(aIVirtualCard, aIVirtualCard.SelfField, playPtn, targetCards);
			if (executeInfo == null || executeInfo.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < executeInfo.Count; j++)
			{
				PlayptnBaseStatsRateInfo playptnBaseStatsRateInfo = executeInfo[j];
				for (int k = 0; k < playptnBaseStatsRateInfo.Targets.Count; k++)
				{
					AIVirtualCard aIVirtualCard2 = playptnBaseStatsRateInfo.Targets[k];
					aIVirtualCard2.MultiplyAttack(playptnBaseStatsRateInfo.Rate);
					aIVirtualCard2.MultiplyLife(playptnBaseStatsRateInfo.Rate);
				}
			}
		}
	}
}
