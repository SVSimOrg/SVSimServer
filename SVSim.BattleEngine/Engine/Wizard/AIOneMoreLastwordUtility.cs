using System.Collections.Generic;

namespace Wizard;

public static class AIOneMoreLastwordUtility
{
	public static bool IsHoldingOneMoreLastword(AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OneMoreLastword))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.OneMoreLastwordTags.CheckCondition(tagOwner, field);
	}

	private static bool IsOneMoreLastwordHolderInplay(AIVirtualField field, List<int> enemyTargets, ref int holderIndex, ref float threshold)
	{
		for (int i = 0; i < enemyTargets.Count; i++)
		{
			AIVirtualCard owner = field.EnemyInplayCards[enemyTargets[i]];
			if (IsGiveTagOfOneMoreLastwordTagged(field, owner, ref threshold))
			{
				holderIndex = i;
				return true;
			}
		}
		return false;
	}

	public static void SortEnemyTargetByBreakBonus(AIVirtualField field, List<int> enemyTargets)
	{
		if (IsHoldingOneMoreLastword(field.EnemyClass, field))
		{
			SortEnemyTargetsWhenLeaderTaggedOneMoreLastword(field, enemyTargets);
			return;
		}
		float threshold = 0f;
		int holderIndex = -1;
		if (IsOneMoreLastwordHolderInplay(field, enemyTargets, ref holderIndex, ref threshold))
		{
			SortEnemyTargetsWhenOneMoreLastwordHolderInplay(field, enemyTargets, holderIndex, threshold);
		}
	}

	private static void SortEnemyTargetsWhenLeaderTaggedOneMoreLastword(AIVirtualField field, List<int> enemyTargets)
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < enemyTargets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.EnemyInplayCards[enemyTargets[i]];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword))
			{
				float num3 = aIVirtualCard.EvaluateBreakValue(field.BestPlayPtn, useIgnoreBreak: true);
				if (num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		if (num2 > 0)
		{
			int value = enemyTargets[num2];
			for (int num4 = num2; num4 > 0; num4--)
			{
				enemyTargets[num4] = enemyTargets[num4 - 1];
			}
			enemyTargets[0] = value;
		}
	}

	private static void SortEnemyTargetsWhenOneMoreLastwordHolderInplay(AIVirtualField field, List<int> enemyTargets, int holderIndex, float breakBonusThreshold)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		float num = float.MaxValue;
		for (int i = 0; i < enemyTargets.Count; i++)
		{
			if (i == holderIndex)
			{
				continue;
			}
			int num2 = enemyTargets[i];
			AIVirtualCard aIVirtualCard = field.EnemyInplayCards[num2];
			if (!aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword) && !aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.Break))
			{
				list2.Add(num2);
				continue;
			}
			float num3 = aIVirtualCard.EvaluateBreakValue(field.BestPlayPtn, useIgnoreBreak: true);
			if (num3 >= breakBonusThreshold)
			{
				list.Add(num2);
			}
			else if (num3 <= num)
			{
				num = num3;
				list2.Insert(0, num2);
			}
			else
			{
				list2.Add(num2);
			}
		}
		list.Add(enemyTargets[holderIndex]);
		enemyTargets.Clear();
		enemyTargets.AddRange(list);
		enemyTargets.AddRange(list2);
	}

	private static bool IsGiveTagOfOneMoreLastwordTagged(AIVirtualField field, AIVirtualCard owner, ref float threshold)
	{
		if (!owner.TagCollectionContainer.HasTag(AIPlayTagType.LastwordAttachTag))
		{
			return false;
		}
		List<AIPlayTag> lastwordAttachTagContents = owner.TagCollectionContainer.LastwordTags.GetLastwordAttachTagContents();
		if (lastwordAttachTagContents == null || lastwordAttachTagContents.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < lastwordAttachTagContents.Count; i++)
		{
			AIPlayTag aIPlayTag = lastwordAttachTagContents[i];
			if (aIPlayTag.Type == AIPlayTagType.OneMoreLastword)
			{
				threshold = aIPlayTag.EvalArg(field.EnemyClass, field.BestPlayPtn, field, null);
				return true;
			}
		}
		return false;
	}
}
