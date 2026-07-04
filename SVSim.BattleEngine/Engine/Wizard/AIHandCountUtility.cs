using System.Collections.Generic;

namespace Wizard;

public static class AIHandCountUtility
{
	public static int GetHandCount(AIVirtualField field, List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
	{
		ReplaceIllegalFilter(filters);
		int num = ((!tagOwner.IsAlly) ? AIFilteringUtility.MultipleFiltering(field.GetEnemyHandCardList(), filters, tagOwner, playPtn, situation) : AIFilteringUtility.MultipleFiltering(field.AllyHandCards, filters, tagOwner, playPtn, situation))?.Count ?? 0;
		if (filters[0] is AIScriptArgumentToken { ArgumentType: AIScriptTokenArgType.ALL })
		{
			num += field.VirtualDrawCount;
		}
		return num;
	}

	public static int GetHandNameCount(AIVirtualField field, List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
	{
		ReplaceIllegalFilter(filters);
		return AIFilteringUtility.GetCardNameCountFromList(tagOwner.IsAlly ? field.AllyHandCards : field.GetEnemyHandCardList(), filters, tagOwner, playPtn, situation);
	}

	private static void ReplaceIllegalFilter(List<AIScriptTokenBase> filters)
	{
		for (int i = 0; i < filters.Count; i++)
		{
			if (filters[i] is AIScriptArgumentToken { ArgumentType: var argumentType } aIScriptArgumentToken)
			{
				switch (argumentType)
				{
				case AIScriptTokenArgType.FOLLOWER:
					aIScriptArgumentToken.ArgumentType = AIScriptTokenArgType.FOLLOWER_CARD_TYPE;
					break;
				case AIScriptTokenArgType.BUFFED_FOLLOWER:
					aIScriptArgumentToken.ArgumentType = AIScriptTokenArgType.BUFFED_FOLLOWER_CARD_TYPE;
					break;
				case AIScriptTokenArgType.SPELL:
					aIScriptArgumentToken.ArgumentType = AIScriptTokenArgType.SPELL_CARD_TYPE;
					break;
				}
			}
		}
	}
}
