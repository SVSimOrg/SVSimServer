using System.Collections.Generic;

namespace Wizard;

public abstract class AITagPreprocessInformationBase
{
	public AIVirtualCard TargetCard { get; protected set; }

	public AITagPreprocessInfoType Type { get; protected set; }

	public AITagPreprocessInformationBase(AIVirtualCard card)
	{
		if (card == null)
		{
			AIConsoleUtility.LogError("AITurnEndStopInformationBase(): Target card is null");
		}
		else
		{
			TargetCard = card;
		}
	}

	public AIVirtualCard GetOverrideTargetCard(List<AIVirtualCard> overrideCardList)
	{
		for (int i = 0; i < overrideCardList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = overrideCardList[i];
			if (aIVirtualCard.IsSameCard(TargetCard))
			{
				return aIVirtualCard;
			}
		}
		return null;
	}

	public virtual AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AITagPreprocessCreationOptionBase(Type, overridedTarget);
	}
}
