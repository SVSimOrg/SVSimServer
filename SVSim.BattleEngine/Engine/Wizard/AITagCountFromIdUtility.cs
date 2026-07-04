using System.Collections.Generic;

namespace Wizard;

public static class AITagCountFromIdUtility
{
	public static int GetTagCountFromId(AIVirtualCard tagOwner, int id, List<AIScriptTokenBase> filters, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			num += GetAttachedTagCount(list[i], id);
		}
		return num;
	}

	public static int GetAttachedTagCount(AIVirtualCard card, int id)
	{
		return card.TagCollectionContainer.AttachedTags?.GetAttachedTagCountFromId(id) ?? 0;
	}
}
