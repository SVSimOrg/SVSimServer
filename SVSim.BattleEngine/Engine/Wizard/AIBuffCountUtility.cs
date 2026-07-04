using System.Collections.Generic;

namespace Wizard;

public static class AIBuffCountUtility
{
	public static int GetBuffCount(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, List<AIScriptTokenBase> argList)
	{
		if (argList.Count < 2)
		{
			return 0;
		}
		AIScriptTokenArgType argumentType = ((AIScriptArgumentToken)argList[0]).ArgumentType;
		AIScriptTokenArgType argumentType2 = ((AIScriptArgumentToken)argList[1]).ArgumentType;
		if (!IsDurationArgLegal(argumentType2) || !IsPositionArgLegal(argumentType))
		{
			return 0;
		}
		argList.RemoveRange(0, 2);
		argList.Reverse();
		if (argumentType == AIScriptTokenArgType.PLAYED)
		{
			return GetPlayedBuffCount(owner, field, argList, situation, playPtn, argumentType2);
		}
		return 0;
	}

	private static int GetPlayedBuffCount(AIVirtualCard owner, AIVirtualField field, List<AIScriptTokenBase> filters, AISituationInfo situation, List<int> playPtn, AIScriptTokenArgType duration)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, filters, owner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			switch (duration)
			{
			case AIScriptTokenArgType.TURN:
				if (aIVirtualCard.BuffRecorderCollection != null)
				{
					int turn = (aIVirtualCard.IsAlly ? field.AllyTurnCount : field.EnemyTurnCount);
					num += aIVirtualCard.BuffRecorderCollection.GetTurnBuffCount(turn, aIVirtualCard.IsSelfTurn);
				}
				break;
			case AIScriptTokenArgType.GAME:
				num += aIVirtualCard.BuffCount;
				break;
			}
		}
		return num;
	}

	private static bool IsDurationArgLegal(AIScriptTokenArgType duration)
	{
		if (duration != AIScriptTokenArgType.TURN)
		{
			return duration == AIScriptTokenArgType.GAME;
		}
		return true;
	}

	private static bool IsPositionArgLegal(AIScriptTokenArgType position)
	{
		return position == AIScriptTokenArgType.PLAYED;
	}
}
