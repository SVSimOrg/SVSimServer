using System.Collections.Generic;

namespace Wizard;

public static class AIBerserkUtility
{
	public static bool IsToBeBerserk(AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isAlly)
	{
		int num = 0;
		if (isAlly && playPtn != null)
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				int index = playPtn[i];
				BattleCardBase baseCard = field.AllyHandCards[index].BaseCard;
				num += AIPlayOnSkillUtility.GetLifePenaltyOnPlay(baseCard, field.AI);
			}
		}
		if (SkillConditionHalfLife.IsHalfLife((isAlly ? field.AllyClass : field.EnemyClass).Life - num) || IsForceBerserk(field, playPtn, situation, isAlly))
		{
			return true;
		}
		return false;
	}

	public static bool IsForceBerserk(AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isAlly)
	{
		List<AIVirtualCard> forceBerserkTagHolders = field.CardListSet.ForceBerserkTagHolders;
		if (forceBerserkTagHolders == null || forceBerserkTagHolders.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < forceBerserkTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = forceBerserkTagHolders[i];
			if (aIVirtualCard.IsAlly == isAlly && aIVirtualCard.TagCollectionContainer.ForceBerserkTags.IsForceBerserk(aIVirtualCard, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}
}
