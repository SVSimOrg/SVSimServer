using System.Collections.Generic;

namespace Wizard;

public class AICannotPlayInformation
{
	public AIVirtualCard Owner;

	public List<AIScriptTokenBase> Filters;

	public AICannotPlayInformation(AIVirtualCard owner, List<AIScriptTokenBase> filters)
	{
		Owner = owner;
		Filters = filters;
	}

	public bool IsEqual(AIVirtualCard card, List<AIScriptTokenBase> filters)
	{
		if (!Owner.IsSameCard(card))
		{
			return false;
		}
		if (!IsSameFilterList(filters))
		{
			return false;
		}
		return true;
	}

	private bool IsSameFilterList(List<AIScriptTokenBase> compare)
	{
		if (Filters == null || Filters.Count <= 0)
		{
			if (compare != null)
			{
				return compare.Count <= 0;
			}
			return true;
		}
		if (Filters.Count != compare.Count)
		{
			return false;
		}
		for (int i = 0; i < Filters.Count; i++)
		{
			if (!Filters[i].IsEqual(compare[i]))
			{
				return false;
			}
		}
		return true;
	}
}
