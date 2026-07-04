using System.Collections.Generic;

namespace Wizard;

public static class AIIsSelectableUtility
{
	public static bool IsSelectable(List<AIScriptTokenBase> filters, float minSelectableCount, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.AllReferableCards, filters, owner, playPtn, situation);
		list.RemoveAll((AIVirtualCard card) => card.IsAlly != owner.IsAlly && card.CantBeFocusedSkill);
		list = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, owner, playPtn);
		if (list == null || (float)list.Count < minSelectableCount)
		{
			return false;
		}
		return true;
	}
}
