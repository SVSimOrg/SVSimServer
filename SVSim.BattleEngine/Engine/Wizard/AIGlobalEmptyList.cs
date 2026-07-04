using System.Collections.Generic;

namespace Wizard;

public static class AIGlobalEmptyList
{
	private static readonly List<AIVirtualCard> _emptyVirtualCardList = new List<AIVirtualCard>();

	public static List<AIVirtualCard> EmptyVirtualCardList
	{
		get
		{
			CheckIsNotEmpty(_emptyVirtualCardList, "EmptyVirtualCardList");
			return _emptyVirtualCardList;
		}
	}

	private static void CheckIsNotEmpty<T>(List<T> targetList, string listName)
	{
		if (targetList.Count > 0)
		{
			targetList.Clear();
		}
	}
}
