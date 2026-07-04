using System.Collections.Generic;

namespace Wizard;

public static class AIPlayPtnUtility
{
	public static bool IsInPlayPtn(AIVirtualCard card, List<int> playPtn)
	{
		AIVirtualField selfField = card.SelfField;
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (selfField.AllyHandCards[playPtn[i]].IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetBeforePlayPtnCount(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return 0;
		}
		int num = -1;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (selfField.AllyHandCards[playPtn[i]].IsSameCard(tagOwner))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return 0;
		}
		return GetPlayPtnCardCount(filters, tagOwner, playPtn.GetRange(0, num), situation);
	}

	public static int GetPlayPtnCardCount(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return 0;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < playPtn.Count; i++)
		{
			list.Add(selfField.AllyHandCards[playPtn[i]]);
		}
		return AIFilteringUtility.MultipleFiltering(list, filters, tagOwner, playPtn, situation)?.Count ?? 0;
	}
}
