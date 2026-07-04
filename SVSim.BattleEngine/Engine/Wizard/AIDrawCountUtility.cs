using System.Collections.Generic;

namespace Wizard;

public static class AIDrawCountUtility
{
	public static int GetDrawCount(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType period, AIScriptTokenArgType range)
	{
		int num = 0;
		switch (range)
		{
		case AIScriptTokenArgType.PLAYPTN:
			return num + GetDrawCountOfPlayptn(owner, field, playPtn);
		case AIScriptTokenArgType.PLAYED:
			return num + GetDrawCountOfPlayed(period, field);
		default:
			num += GetDrawCountOfPlayptn(owner, field, playPtn);
			return num + GetDrawCountOfPlayed(period, field);
		}
	}

	private static int GetDrawCountOfPlayptn(AIVirtualCard owner, AIVirtualField field, List<int> playPtn)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return 0;
		}
		AISinglePlayptnRecord playptnRecordOnSim = field.GetPlayptnRecordOnSim(playPtn);
		if (playptnRecordOnSim == null)
		{
			return 0;
		}
		int num = 0;
		List<PlayedCardInfo> playedCardList = playptnRecordOnSim.PlayedCardList;
		for (int i = 0; i < playedCardList.Count && !playedCardList[i].Card.IsSameCard(owner); i++)
		{
			num += playedCardList[i].DrawCount;
		}
		return num;
	}

	private static int GetDrawCountOfPlayed(AIScriptTokenArgType period, AIVirtualField field)
	{
		int virtualDrawCount = field.VirtualDrawCount;
		return period switch
		{
			AIScriptTokenArgType.TURN => virtualDrawCount + field.TurnDrawCount, 
			AIScriptTokenArgType.GAME => virtualDrawCount + field.GameDrawCount, 
			_ => 0, 
		};
	}
}
