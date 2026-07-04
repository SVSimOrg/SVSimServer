using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public static class AITargetSelectFilteringUtility
{
	public static List<AIVirtualCard> FilteringWithTargetTags(this AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null)
		{
			return new List<AIVirtualCard>();
		}
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.Target))
		{
			return candidates;
		}
		return tagOwner.TagCollectionContainer.TargetTags.FilteringTargetCards(tagOwner, candidates, playPtn, situation);
	}

	public static List<AIVirtualCard> FilteringIgnoreTargets(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<int> playPtn, AISituationInfo situation, int selectCount)
	{
		if (tagOwner == null)
		{
			return new List<AIVirtualCard>();
		}
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.IgnoreTarget))
		{
			return candidates;
		}
		return tagOwner.TagCollectionContainer.IgnoreTargetTags.FilteringIgnoreTargets(tagOwner, candidates, playPtn, situation, selectCount);
	}

	public static bool IsOnlyIgnoreTarget(AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AISinglePlayptnRecord playptnRecord)
	{
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(playCard.FindRealActor(playptnRecord), playCard, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
		List<AIVirtualTargetSelectInfo> list = playCard.CreateAIVirtualSelectInfo(field, situation);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = list[i];
			if (FilteringIgnoreTargets(playCard, aIVirtualTargetSelectInfo.Candidates, playPtn, situation, aIVirtualTargetSelectInfo.Count).Count <= 0)
			{
				return true;
			}
		}
		return false;
	}

	public static List<AIVirtualCard> SelectCandidatesWithForceTargeting(List<AIVirtualCard> selectableCards, AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (selectableCards == null || selectableCards.Count <= 0)
		{
			return AIGlobalEmptyList.EmptyVirtualCardList;
		}
		List<AIVirtualCard> result = selectableCards;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < selectableCards.Count; i++)
		{
			if (tagOwner.IsAlly != selectableCards[i].IsAlly && selectableCards[i].IsForceTargeting)
			{
				if (list == null)
				{
					list = new List<AIVirtualCard>();
				}
				list.Add(selectableCards[i]);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list;
		}
		return result;
	}

	public static List<AIVirtualCard> ExecuteTargetFilteringTagToRealCardList(AIVirtualCard virtualActCard, IEnumerable<BattleCardBase> selectableCards, AIVirtualField field, List<int> playPtn)
	{
		List<AIVirtualCard> targets = new List<AIVirtualCard>(selectableCards.Select((BattleCardBase c) => field.SearchVirtualCard(c)));
		return ExecuteTargetFilteringTags(virtualActCard, targets, playPtn, null);
	}

	public static List<AIVirtualCard> ExecuteTargetFilteringTags(AIVirtualCard actCard, List<AIVirtualCard> targets, List<int> playPtn, AISituationInfo situation, int selectCount = 1)
	{
		List<AIVirtualCard> candidates = targets;
		List<AIVirtualCard> list = FilteringIgnoreTargets(actCard, candidates, playPtn, situation, selectCount);
		if (list.IsNotNullOrEmpty())
		{
			candidates = list;
		}
		return actCard.FilteringWithTargetTags(candidates, playPtn, situation);
	}

	public static List<AIVirtualCard> GetLegalCandidates(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, int selectCount)
	{
		candidates.RemoveAll((AIVirtualCard c) => c.IsAlly != tagOwner.IsAlly && c.CantBeFocusedSkill);
		List<AIVirtualCard> list = null;
		for (int num = 0; num < candidates.Count; num++)
		{
			AIVirtualCard aIVirtualCard = candidates[num];
			if (tagOwner.IsAlly != aIVirtualCard.IsAlly && aIVirtualCard.IsForceTargeting)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list != null && list.Count > 0 && list.Count >= selectCount)
		{
			return list;
		}
		return candidates;
	}

	public static AISelectedTargetInfo GetRuleBaseTargets(AIPlayTag rule, List<AIVirtualCard> candidates, AIVirtualField field, AIVirtualTargetSelectAction situation, AIRemovalType removalType)
	{
		List<AIVirtualCard> targetList = null;
		AIVirtualCard actor = situation.Actor;
		switch (rule.Type)
		{
		case AIPlayTagType.FanfareHandBuff:
		case AIPlayTagType.PlayHandBuff:
		case AIPlayTagType.FanfareSpellboost:
		case AIPlayTagType.PlaySpellboost:
		case AIPlayTagType.PlayChangeCost:
		case AIPlayTagType.FanfareChangeCost:
			(rule.ArgumentExpressions as AIWhenPlayTagArgument).RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
			break;
		case AIPlayTagType.EvoHandBuff:
		case AIPlayTagType.EvoDiscard:
		case AIPlayTagType.EvoChangeCost:
			(rule.ArgumentExpressions as AIEvoTagArgument).RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
			break;
		}
		if (targetList == null || targetList.Count <= 0)
		{
			return null;
		}
		return new AISelectedTargetInfo(targetList, TargetSelectType.NormalRuleBase, removalType);
	}

	public static int GetForbiddenSelectedTargetFilterIndex(List<AIScriptTokenBase> filterList)
	{
		if (filterList == null || filterList.Count <= 0)
		{
			return -1;
		}
		for (int i = 0; i < filterList.Count; i++)
		{
			if (filterList[i] is AIScriptArgumentToken { ArgumentType: AIScriptTokenArgType.SELECTED_TARGET, IsNot: not false })
			{
				return i;
			}
		}
		return -1;
	}
}
