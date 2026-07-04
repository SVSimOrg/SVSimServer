using System.Collections.Generic;

namespace Wizard;

public static class AIRallySimulationUtility
{
	public static void ExecuteAppendRallyCount(AIVirtualField field, AIVirtualCard summonCard)
	{
		if (summonCard != null && CanAppendRallyCount(field, summonCard))
		{
			field.AddRallyCount(1, summonCard.IsAlly);
		}
	}

	public static bool CanAppendRallyCount(AIVirtualField field, AIVirtualCard summonCard)
	{
		if (summonCard.IsUnit)
		{
			return true;
		}
		List<AIVirtualCard> tagHolders = field.CardListSet.GetTagHolders(CardListsForReference.TagHolderReferenceType.RallyCountPlus);
		if (tagHolders == null)
		{
			return false;
		}
		for (int i = 0; i < tagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = tagHolders[i];
			if (aIVirtualCard.TagCollectionContainer.RallyCountPlusTags.CanAppendRallyCount(aIVirtualCard, summonCard))
			{
				return true;
			}
		}
		return false;
	}
}
