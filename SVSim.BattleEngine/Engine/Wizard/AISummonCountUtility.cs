using System.Collections.Generic;

namespace Wizard;

public static class AISummonCountUtility
{
	public static bool ClassificateSummonCountArgument(List<AIScriptTokenBase> argList, out List<AIScriptTokenBase> filters, out AIScriptTokenArgType turnOrGame, out AIScriptTokenArgType playedType)
	{
		filters = new List<AIScriptTokenBase>();
		turnOrGame = AIScriptTokenArgType.NONE;
		playedType = AIScriptTokenArgType.NONE;
		if (argList == null || argList.Count < 2)
		{
			return false;
		}
		bool flag = false;
		if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken)
		{
			if (aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.PLAYED || aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.PLAYPTN)
			{
				playedType = aIScriptArgumentToken.ArgumentType;
				flag = true;
			}
			else if (aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.TURN || aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.GAME)
			{
				turnOrGame = aIScriptArgumentToken.ArgumentType;
			}
			argList.RemoveAt(0);
			if (flag)
			{
				if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken2 && (aIScriptArgumentToken2.ArgumentType == AIScriptTokenArgType.TURN || aIScriptArgumentToken2.ArgumentType == AIScriptTokenArgType.GAME))
				{
					turnOrGame = aIScriptArgumentToken2.ArgumentType;
				}
				argList.RemoveAt(0);
			}
			if (turnOrGame == AIScriptTokenArgType.NONE)
			{
				return false;
			}
			argList.Reverse();
			filters = argList;
			return true;
		}
		return false;
	}

	public static int GetSummonCount(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, AIScriptTokenArgType turnOrGame, AIScriptTokenArgType playedType, List<int> playPtn, AISituationInfo situation)
	{
		if (turnOrGame != AIScriptTokenArgType.TURN && turnOrGame != AIScriptTokenArgType.GAME)
		{
			return 0;
		}
		return playedType switch
		{
			AIScriptTokenArgType.NONE => GetSummonCountAtPlayed(tagOwner, filters, turnOrGame, playPtn, situation) + GetSummonCountAtPlayPtn(tagOwner, filters, playPtn, situation), 
			AIScriptTokenArgType.PLAYED => GetSummonCountAtPlayed(tagOwner, filters, turnOrGame, playPtn, situation), 
			AIScriptTokenArgType.PLAYPTN => GetSummonCountAtPlayPtn(tagOwner, filters, playPtn, situation), 
			_ => 0, 
		};
	}

	private static int GetSummonCountAtPlayed(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, AIScriptTokenArgType turnOrGame, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.SummonedCardContainer.GetSummonedList(tagOwner.IsAlly, turnOrGame == AIScriptTokenArgType.TURN), filters, tagOwner, playPtn, situation)?.Count ?? 0;
	}

	private static int GetSummonCountAtPlayPtn(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.IsAlly || playPtn == null || playPtn.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		List<AIVirtualCard> allyHandCards = tagOwner.SelfField.AllyHandCards;
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (playPtn[i] < allyHandCards.Count)
			{
				AIVirtualCard aIVirtualCard = allyHandCards[playPtn[i]];
				if ((((aIVirtualCard.IsUnit || aIVirtualCard.IsAmulet) && !aIVirtualCard.IsAccelerated(tagOwner.SelfField, playPtn, situation)) || aIVirtualCard.IsCrystalize(tagOwner.SelfField, playPtn, situation)) && AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, null, filters, playPtn, tagOwner, situation))
				{
					num++;
				}
			}
		}
		return num;
	}
}
