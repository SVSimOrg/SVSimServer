using System.Collections.Generic;

namespace Wizard;

public static class AIChoiceTransformUtility
{
	public static int GetChoiceTransformCost(AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (card == null || card.ChoiceTransformCostList == null || !card.IsInHand)
		{
			return -1;
		}
		List<int> playPtn2 = (card.IsAlly ? playPtn : null);
		return field.AI.PlayPtnRecorder.GetCardPlaySimulationTypeCost(card, field, playPtn2, null, PlaySimulationType.ChoiceTransform);
	}

	public static bool IsChoiceTransform(AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		if (situation.ActionType != AIOperationType.PLAY)
		{
			return false;
		}
		AIVirtualCard originalCard = situation.OriginalCard;
		if (originalCard.ChoiceTransformCostList == null || originalCard.ChoiceTransformCostList.Count <= 0)
		{
			return false;
		}
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		int num = (originalCard.IsAlly ? field.AllyPp : field.EnemyPp);
		int num2 = -1;
		for (int i = 0; i < originalCard.ChoiceTransformCostList.Count; i++)
		{
			AIChoiceTransformCostInformation aIChoiceTransformCostInformation = originalCard.ChoiceTransformCostList[i];
			if (aIChoiceTransformCostInformation.CheckCondition(originalCard, field, emptyPlayPtn, situation))
			{
				int num3 = (int)aIChoiceTransformCostInformation.Cost.EvalArg(originalCard, emptyPlayPtn, field, situation);
				if (num >= num3 && num3 > num2)
				{
					num2 = num3;
				}
			}
		}
		return num2 >= 0;
	}
}
