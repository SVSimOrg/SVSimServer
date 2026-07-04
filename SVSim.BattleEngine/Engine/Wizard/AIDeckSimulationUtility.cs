using System.Collections.Generic;

namespace Wizard;

public static class AIDeckSimulationUtility
{
	public static List<AIVirtualCard> GetFilteredDeck(List<AIScriptTokenBase> filter, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.IsAlly ? field.AI.AllyDeckCards : field.AI.EnemyDeckCards, filter, tagOwner, playPtn, situation);
		if (list == null)
		{
			list = new List<AIVirtualCard>();
		}
		List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(field.DummyDeckContainer.GetDeck(tagOwner.IsAlly), filter, tagOwner, playPtn, situation);
		if (list2 != null)
		{
			list.AddRange(list2);
		}
		return list;
	}
}
