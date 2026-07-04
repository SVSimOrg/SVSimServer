using System.Collections.Generic;

namespace Wizard;

public static class AICannotPlaySimulationUtility
{
	public static bool IsCannotPlayByTag(this AIVirtualField field, AIVirtualTargetSelectAction situation, List<int> playPtn)
	{
		if (situation.ActionType != AIOperationType.PLAY || !situation.OriginalCard.IsAlly)
		{
			return false;
		}
		if (field.CannotPlayInformationList == null || field.CannotPlayInformationList.Count <= 0)
		{
			return false;
		}
		AIVirtualCard originalCard = situation.OriginalCard;
		for (int i = 0; i < field.CannotPlayInformationList.Count; i++)
		{
			AICannotPlayInformation aICannotPlayInformation = field.CannotPlayInformationList[i];
			if (AIFilteringUtility.CheckMatchTargetFiltering(originalCard, field.AllyHandCards, aICannotPlayInformation.Filters, playPtn, aICannotPlayInformation.Owner, situation))
			{
				return true;
			}
		}
		return false;
	}
}
