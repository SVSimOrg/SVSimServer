using System.Collections.Generic;

namespace Wizard;

public static class AIGetStatusUtility
{
	public static int GetLife(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.AllReferableCards, filters, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		return list[0].Life;
	}

	public static int GetAttack(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.AllReferableCards, filters, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		return list[0].Attack;
	}
}
