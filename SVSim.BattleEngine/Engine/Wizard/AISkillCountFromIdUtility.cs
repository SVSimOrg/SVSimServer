using System.Collections.Generic;

namespace Wizard;

public static class AISkillCountFromIdUtility
{
	public static int GetSkillCountFromID(List<AIScriptTokenBase> filters, int skillOwnerId, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			num += GetAttachedSkillCount(list[i].BaseCard, skillOwnerId);
		}
		return num;
	}

	public static int GetAttachedSkillCount(BattleCardBase targetCard, int skillOwnerId)
	{
		int num = 0;
		List<int> ownerCardIdList = targetCard.SkillApplyInformation.AttachedSkillsInfo.OwnerCardIdList;
		for (int i = 0; i < ownerCardIdList.Count; i++)
		{
			if (EnemyAI.GetBaseId(ownerCardIdList[i]) == skillOwnerId)
			{
				num++;
			}
		}
		return num;
	}
}
