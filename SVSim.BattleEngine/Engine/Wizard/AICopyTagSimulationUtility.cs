using System.Collections.Generic;

namespace Wizard;

public static class AICopyTagSimulationUtility
{
	public static List<AIPlayTag> GetCopyTagListFromCard(AIVirtualCard target, List<AIScriptTokenArgType> timingList)
	{
		List<AIPlayTag> list = null;
		for (int i = 0; i < timingList.Count; i++)
		{
			AIScriptTokenArgType timing = timingList[i];
			List<AIPlayTag> copyTagOfCertainTiming = GetCopyTagOfCertainTiming(target, timing);
			if (copyTagOfCertainTiming != null && copyTagOfCertainTiming.Count > 0)
			{
				list = AIParamQuery.AddRangeToList(copyTagOfCertainTiming, list);
			}
		}
		return list;
	}

	public static void ExecuteCopyAndAttachTagToAll(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, List<AIScriptTokenArgType> skillTimingList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (targetList == null || targetList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targetList.Count; i++)
		{
			List<AIPlayTag> copyTagListFromCard = GetCopyTagListFromCard(targetList[i], skillTimingList);
			if (copyTagListFromCard != null)
			{
				AttachTagFromCopyTagList(tagOwner, tagOwner, copyTagListFromCard, situation);
			}
		}
	}

	public static void ExecuteCopyAndAttachTagToSelectedTarget(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<AIScriptTokenArgType> skillTimingList, AIScriptTokenArgType whichTarget, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(whichTarget))
		{
			CopyAndAttachTagToSituationTarget(tagOwner, candidates, skillTimingList, whichTarget, field, playPtn, situation);
		}
		else
		{
			CopyAndAttachTagPrediction(tagOwner, candidates, skillTimingList, field, playPtn, situation);
		}
	}

	private static void CopyAndAttachTagToSituationTarget(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<AIScriptTokenArgType> skillTimingList, AIScriptTokenArgType whichTarget, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.Targets != null)
		{
			ExecuteCopyAndAttachTagToAll(tagOwner, situationTarget.Targets, skillTimingList, field, playPtn, situation);
		}
	}

	private static void CopyAndAttachTagPrediction(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<AIScriptTokenArgType> skillTimingList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		AIVirtualCard aIVirtualCard = SelecteBestCopyTarget(tagOwner, candidates, field, playPtn, situation);
		if (aIVirtualCard != null)
		{
			List<AIPlayTag> copyTagListFromCard = GetCopyTagListFromCard(aIVirtualCard, skillTimingList);
			if (copyTagListFromCard != null)
			{
				AttachTagFromCopyTagList(tagOwner, tagOwner, copyTagListFromCard, situation);
			}
		}
	}

	private static AIVirtualCard SelecteBestCopyTarget(AIVirtualCard attachTarget, List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		AIVirtualCard result = null;
		float num = float.MinValue;
		for (int i = 0; i < candidates.Count; i++)
		{
			float num2 = 0f;
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
			{
				num2 += (float)aIVirtualCard.TagCollectionContainer.AttackTags.TagList.Count * 1f;
			}
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword))
			{
				num2 += (float)aIVirtualCard.TagCollectionContainer.LastwordTags.TagList.Count * 1f;
			}
			num2 += (aIVirtualCard.IsQuick ? 3f : 0f);
			num2 += (aIVirtualCard.IsKiller ? 1f : 0f);
			num2 += (aIVirtualCard.IsDrain ? 1f : 0f);
			num2 += (aIVirtualCard.IsGuard ? 1f : 0f);
			if (aIVirtualCard.IsRush)
			{
				List<AIVirtualCard> list = (attachTarget.IsAlly ? field.EnemyInplayCards : field.AllyInplayCards);
				num2 += ((list != null && list.Count > 0) ? 1f : 0.5f);
			}
			if (num2 > num)
			{
				num = num2;
				result = aIVirtualCard;
			}
		}
		return result;
	}

	private static void AttachTagFromCopyTagList(AIVirtualCard tagOwner, AIVirtualCard attachTarget, List<AIPlayTag> copyTgList, AISituationInfo situation)
	{
		for (int i = 0; i < copyTgList.Count; i++)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToSingle(attachTarget, tagOwner, copyTgList[i], AIScriptTokenArgType.NONE, situation);
		}
	}

	private static List<AIPlayTag> GetCopyTagOfCertainTiming(AIVirtualCard target, AIScriptTokenArgType timing)
	{
		switch (timing)
		{
		case AIScriptTokenArgType.KILLER:
		case AIScriptTokenArgType.QUICK:
		case AIScriptTokenArgType.RUSH:
		case AIScriptTokenArgType.DRAIN:
		case AIScriptTokenArgType.GUARD:
			return CopyKeywordSkillTags(target, timing);
		case AIScriptTokenArgType.WHEN_DESTROY:
			return CopyWhenDestroyTags(target);
		case AIScriptTokenArgType.WHEN_ATTACK:
			return CopyWhenAttackTags(target);
		case AIScriptTokenArgType.WHEN_CLASH:
			return CopyWhenClashTags(target);
		default:
			return null;
		}
	}

	private static List<AIPlayTag> CopyKeywordSkillTags(AIVirtualCard target, AIScriptTokenArgType skillType)
	{
		List<AIPlayTag> list = null;
		if (target.IsHoldKeywordSkill(skillType))
		{
			AIPlayTag aIPlayTag = AIPlayTagInitializingUtility.CreateBasicSkillTag(skillType);
			if (aIPlayTag == null)
			{
				return null;
			}
			list = AIParamQuery.AddElementToList(aIPlayTag, list);
		}
		return list;
	}

	private static List<AIPlayTag> CopyWhenDestroyTags(AIVirtualCard target)
	{
		List<AIPlayTag> list = null;
		if (target.TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword))
		{
			list = AIParamQuery.AddRangeToList(target.TagCollectionContainer.LastwordTags.TagList, list);
		}
		if (target.TagCollectionContainer.HasTag(AIPlayTagType.Break))
		{
			list = AIParamQuery.AddRangeToList(target.TagCollectionContainer.BreakBonusTags.TagList, list);
		}
		if (target.TagCollectionContainer.HasTag(AIPlayTagType.IgnoreBreak))
		{
			list = AIParamQuery.AddRangeToList(target.TagCollectionContainer.IgnoreBreakTags.TagList, list);
		}
		return list;
	}

	private static List<AIPlayTag> CopyWhenAttackTags(AIVirtualCard target)
	{
		if (target.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			return target.TagCollectionContainer.AttackTags.TagList;
		}
		return null;
	}

	private static List<AIPlayTag> CopyWhenClashTags(AIVirtualCard target)
	{
		if (target.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			AttackTagCollection attackTags = target.TagCollectionContainer.AttackTags;
			if (attackTags.HasClashTag)
			{
				return attackTags.ClashTags;
			}
		}
		return null;
	}
}
