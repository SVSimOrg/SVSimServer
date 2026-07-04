using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public static class AIDiscardUtility
{
	private class DiscardCandidateOrder : IComparer<Tuple<AIVirtualCard, float>>
	{
		public enum SortType
		{
			Ascending,
			Dscending
		}

		private SortType _sortOrder;

		public DiscardCandidateOrder(SortType sortType)
		{
			_sortOrder = sortType;
		}

		public int Compare(Tuple<AIVirtualCard, float> left, Tuple<AIVirtualCard, float> right)
		{
			if (left.second > right.second)
			{
				if (_sortOrder != SortType.Ascending)
				{
					return -1;
				}
				return 1;
			}
			if (left.second < right.second)
			{
				if (_sortOrder != SortType.Ascending)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}
	}

	public static float CalcAllDiscardedBonus(AIVirtualField field, AISituationInfo playSituation, List<int> playPtn)
	{
		AIDiscardInfo discardInfo = playSituation.DiscardInfo;
		if (discardInfo == null || !discardInfo.IsValuable)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < discardInfo.TargetList.Count; i++)
		{
			num += EvaluateDiscardedBonus(discardInfo.TargetList[i], playPtn, playSituation, field, isIgnoreInBattle: false, isCalcCostDiff: false, isCalcTokenValue: false);
		}
		if (field.CardListSet.HasAfterDiscardTagHolder)
		{
			for (int j = 0; j < field.CardListSet.AfterDiscardTagHolders.Count; j++)
			{
				AIVirtualCard tagOwner = field.CardListSet.AfterDiscardTagHolders[j];
				num += tagOwner.GetAllyDiscardBonus(playPtn, playSituation, useIgnoreInBattle: false);
			}
		}
		return num;
	}

	public static float EvaluateDiscardedBonus(AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation, AIVirtualField field, bool isIgnoreInBattle, bool isCalcCostDiff, bool isCalcTokenValue)
	{
		float num = 0f;
		num += targetCard.GetDiscardedBonus(playPtn, situation, isIgnoreInBattle);
		if (isCalcTokenValue)
		{
			num += targetCard.GetDiscardedTokenBonus(field, playPtn, situation, isIgnoreInBattle);
		}
		num -= targetCard.GetHandBonus(playPtn, situation, isIgnoreInFusion: false);
		if (isCalcCostDiff)
		{
			num += (float)Mathf.Abs(targetCard.SelfField.AllyPpTotal - targetCard.Cost) * 0.001f;
		}
		if (playPtn != null && playPtn.Count > 0)
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				if (field.AllyHandCards[playPtn[i]].IsSameCard(targetCard))
				{
					num += -1000f;
					break;
				}
			}
		}
		return num;
	}

	public static List<AITokenInformation> GetAllDiscardedTokenIds(AIDiscardInfo discardInfo, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (discardInfo == null || !discardInfo.IsValuable)
		{
			return null;
		}
		List<AITokenInformation> list = null;
		for (int i = 0; i < discardInfo.TargetList.Count; i++)
		{
			List<AITokenInformation> discardedTokenIds = discardInfo.TargetList[i].GetDiscardedTokenIds(field, playPtn, situation);
			if (discardedTokenIds != null)
			{
				list = AIParamQuery.AddRangeToList(discardedTokenIds, list);
			}
		}
		return list;
	}

	public static List<AIVirtualCard> SelectBestDiscardTarget(AIVirtualCard tagOwner, AIVirtualField field, List<AIVirtualCard> discardCandidates, int discardCount, List<int> playPtn, AISituationInfo situation)
	{
		return SelectDiscardTargets(tagOwner, field, discardCandidates, discardCount, playPtn, situation, DiscardCandidateOrder.SortType.Dscending);
	}

	public static List<AIVirtualCard> SelectWorstDiscardTarget(AIVirtualCard tagOwner, AIVirtualField field, List<AIVirtualCard> discardCandidates, int discardCount, List<int> playPtn, AISituationInfo situation)
	{
		return SelectDiscardTargets(tagOwner, field, discardCandidates, discardCount, playPtn, situation, DiscardCandidateOrder.SortType.Ascending);
	}

	private static List<AIVirtualCard> SelectDiscardTargets(AIVirtualCard tagOwner, AIVirtualField field, List<AIVirtualCard> discardCandidates, int discardCount, List<int> playPtn, AISituationInfo situation, DiscardCandidateOrder.SortType sortType)
	{
		if (discardCandidates == null)
		{
			return null;
		}
		List<AIVirtualCard> list;
		if (discardCandidates.Count <= discardCount)
		{
			list = new List<AIVirtualCard>(discardCandidates.Count);
			for (int i = 0; i < discardCandidates.Count; i++)
			{
				list.Add(discardCandidates[i]);
			}
			return list;
		}
		list = SortDiscardCandidats(discardCandidates, playPtn, situation, field, sortType);
		return list.GetRange(0, discardCount);
	}

	private static List<AIVirtualCard> SortDiscardCandidats(List<AIVirtualCard> candidates, List<int> playPtn, AISituationInfo situation, AIVirtualField field, DiscardCandidateOrder.SortType sortType)
	{
		List<Tuple<AIVirtualCard, float>> list = new List<Tuple<AIVirtualCard, float>>();
		for (int i = 0; i < candidates.Count; i++)
		{
			list.Add(new Tuple<AIVirtualCard, float>
			{
				first = candidates[i],
				second = EvaluateDiscardedBonus(candidates[i], playPtn, situation, field, isIgnoreInBattle: false, isCalcCostDiff: true, isCalcTokenValue: true)
			});
		}
		DiscardCandidateOrder comparer = new DiscardCandidateOrder(sortType);
		list.Sort(comparer);
		List<AIVirtualCard> list2 = new List<AIVirtualCard>(list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			list2.Add(list[j].first);
		}
		return list2;
	}

	public static bool IsMatchedDiscardTarget(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || situation == null)
		{
			return false;
		}
		AIDiscardInfo discardInfo = situation.DiscardInfo;
		if (discardInfo == null || !discardInfo.IsValuable)
		{
			return false;
		}
		List<AIVirtualCard> targetList = discardInfo.TargetList;
		for (int i = 0; i < targetList.Count; i++)
		{
			if (AIFilteringUtility.CheckMatchTargetFiltering(targetList[i], targetList, filters, playPtn, tagOwner, situation))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetDiscardCount(AIScriptTokenArgType discardListType, AIVirtualCard ownerCard, List<int> playPtn, AISituationInfo situation)
	{
		AIVirtualField selfField = ownerCard.SelfField;
		switch (discardListType)
		{
		case AIScriptTokenArgType.PLAYED:
			return GetDiscardCountFromDiscardedList(selfField);
		case AIScriptTokenArgType.BEFORE_PLAYPTN:
			return GetDiscardCountFromBeforePlayPtnNew(ownerCard, selfField, playPtn, selfField.AI.PlayPtnRecorder);
		case AIScriptTokenArgType.NOW:
		{
			AIDiscardInfo discardInfo = situation.DiscardInfo;
			if (discardInfo == null || !discardInfo.IsValuable)
			{
				return 0;
			}
			return discardInfo.TargetList.Count;
		}
		default:
			return 0;
		}
	}

	private static int GetDiscardCountFromDiscardedList(AIVirtualField field)
	{
		return field.AI.DiscardedCards.Count((AIVirtualCard c) => c.IsSelfTurn == field.AllyBattlePlayer.IsSelfTurn && field.AI.BattleMgr.CurrentTurn == c.DestroyedTurn);
	}

	private static int GetDiscardCountFromBeforePlayPtnNew(AIVirtualCard ownerCard, AIVirtualField field, List<int> playPtn, AIPlayptnRecorder recorder)
	{
		AISinglePlayptnRecord playptnRecordOnSim = field.GetPlayptnRecordOnSim(playPtn);
		if (playptnRecordOnSim == null)
		{
			AIConsoleUtility.LogError("GetDiscardCountFromPlayPtn error!! Cannot find playPtn record!!!!!");
			return 0;
		}
		int num = 0;
		List<PlayedCardInfo> playedCardList = playptnRecordOnSim.PlayedCardList;
		for (int i = 0; i < playedCardList.Count; i++)
		{
			PlayedCardInfo playedCardInfo = playedCardList[i];
			if (playedCardInfo.Card.IsSameCard(ownerCard))
			{
				break;
			}
			if (playedCardInfo.DiscardInfo != null && playedCardInfo.DiscardInfo.TargetList != null)
			{
				num += playedCardInfo.DiscardInfo.TargetList.Count;
			}
		}
		return num;
	}

	public static bool CheckAttackDiscardTargetInPlayPtn(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation.ActionType != AIOperationType.ATTACK)
		{
			return false;
		}
		AIVirtualCard actor = situation.Actor;
		if (!actor.TagCollectionContainer.HasTag(AIPlayTagType.AttackDiscard))
		{
			return false;
		}
		if (actor.TagCollectionContainer.AttackTags.CheckAttackDiscardTargetInPlayPtn(actor, field.AllyHandCards, playPtn, situation))
		{
			return true;
		}
		return false;
	}
}
