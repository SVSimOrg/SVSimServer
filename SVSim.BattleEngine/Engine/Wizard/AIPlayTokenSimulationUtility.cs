using System.Collections.Generic;
using Cute;

namespace Wizard;

public static class AIPlayTokenSimulationUtility
{
	public static List<AITokenInformation> GetSingleCardAllySideWhenPlayTokenIds(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AITokenInformation> list = null;
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("AIPlayTokenSimulationUtility.GetSingleCardAllySideWhenPlayTokenIds() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			FanfareTagCollection fanfareTags = actor.TagCollectionContainer.FanfareTags;
			if (fanfareTags.HasSummonTokenTags)
			{
				List<AITokenInformation> allySideWhenPlaySummonTokenIdList = fanfareTags.GetAllySideWhenPlaySummonTokenIdList(actor, field, playPtn, situation);
				if (allySideWhenPlaySummonTokenIdList != null && allySideWhenPlaySummonTokenIdList.Count > 0)
				{
					list = AIParamQuery.AddRangeToList(allySideWhenPlaySummonTokenIdList, list);
				}
			}
		}
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			PlayTagCollection playTags = actor.TagCollectionContainer.PlayTags;
			if (playTags.HasSummonTokenTags)
			{
				List<AITokenInformation> allySideWhenPlaySummonTokenIdList2 = playTags.GetAllySideWhenPlaySummonTokenIdList(actor, field, playPtn, situation);
				if (allySideWhenPlaySummonTokenIdList2 != null && allySideWhenPlaySummonTokenIdList2.Count > 0)
				{
					list = AIParamQuery.AddRangeToList(allySideWhenPlaySummonTokenIdList2, list);
				}
			}
		}
		return list;
	}

	public static List<AITokenInformation> GetAllySideOtherPlayTokenIds(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherPlayToken))
		{
			return null;
		}
		return tagOwner.TagCollectionContainer.OtherPlayTags.GetAllySideTokenIds(tagOwner, field, playPtn, situation);
	}

	public static AITokenIdCollection GetSingleCardBothSideWhenPlayTokenIds(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("AIPlayTokenSimulationUtility.GetSingleCardBothSideWhenPlayTokenIds() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		AITokenIdCollection aITokenIdCollection = null;
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			FanfareTagCollection fanfareTags = actor.TagCollectionContainer.FanfareTags;
			if (fanfareTags.HasSummonTokenTags)
			{
				aITokenIdCollection = fanfareTags.GetBothSideWhenPlaySummonTokenIdList(actor, field, playPtn, situation);
			}
		}
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			PlayTagCollection playTags = actor.TagCollectionContainer.PlayTags;
			if (playTags.HasSummonTokenTags)
			{
				AITokenIdCollection bothSideWhenPlaySummonTokenIdList = playTags.GetBothSideWhenPlaySummonTokenIdList(actor, field, playPtn, situation);
				if (bothSideWhenPlaySummonTokenIdList != null && bothSideWhenPlaySummonTokenIdList.HasToken)
				{
					aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, bothSideWhenPlaySummonTokenIdList);
				}
			}
		}
		return aITokenIdCollection;
	}

	public static AITokenIdCollection GetOtherPlayTokenIdCollection(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherPlayToken))
		{
			return null;
		}
		return tagOwner.TagCollectionContainer.OtherPlayTags.GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation);
	}

	public static List<AITokenInformation> GetAllySideTokenIdsOfPlaySituation(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("AIPlayTokenSimulationUtility.GetAllySideTokenIdsOfPlaySituation() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		List<AITokenInformation> list = null;
		List<AITokenInformation> singleCardAllySideWhenPlayTokenIds = GetSingleCardAllySideWhenPlayTokenIds(field, playPtn, situation);
		if (singleCardAllySideWhenPlayTokenIds != null)
		{
			list = AIParamQuery.AddRangeToList(singleCardAllySideWhenPlayTokenIds, list);
		}
		List<AIVirtualCard> list2 = new List<AIVirtualCard>(field.CardListSet.BothClassAndInplayCards);
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[i]];
			if (aIVirtualCard.IsSameCard(situation.OriginalCard))
			{
				break;
			}
			if (!aIVirtualCard.IsSpell && !aIVirtualCard.IsAccelerated(field, playPtn.GetRange(0, i + 1), situation))
			{
				list2.Add(aIVirtualCard);
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			singleCardAllySideWhenPlayTokenIds = GetAllySideOtherPlayTokenIds(list2[j], field, playPtn, situation);
			if (singleCardAllySideWhenPlayTokenIds != null && singleCardAllySideWhenPlayTokenIds.Count > 0)
			{
				list = AIParamQuery.AddRangeToList(singleCardAllySideWhenPlayTokenIds, list);
			}
		}
		return list;
	}

	public static AITokenIdCollection GetBothSideTokenIdsOfPlaySituation(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("AIPlayTokenSimulationUtility.GetBothSideTokenIdsOfPlaySituation() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		AITokenIdCollection aITokenIdCollection = GetSingleCardBothSideWhenPlayTokenIds(field, playPtn, situation);
		List<AIVirtualCard> list = new List<AIVirtualCard>(field.CardListSet.BothClassAndInplayCards);
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[i]];
			if (aIVirtualCard.IsSameCard(situation.OriginalCard))
			{
				break;
			}
			if (!aIVirtualCard.IsSpell && !aIVirtualCard.IsAccelerated(field, playPtn.GetRange(0, i + 1), situation))
			{
				list.Add(aIVirtualCard);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			AITokenIdCollection otherPlayTokenIdCollection = GetOtherPlayTokenIdCollection(list[j], field, playPtn, situation);
			if (otherPlayTokenIdCollection != null && otherPlayTokenIdCollection.HasToken)
			{
				aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, otherPlayTokenIdCollection);
			}
		}
		return aITokenIdCollection;
	}

	public static int GetPlayTokenCount(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		AIVirtualField selfField = tagOwner.SelfField;
		AISinglePlayptnRecord playptnRecordOnSim = selfField.GetPlayptnRecordOnSim(playPtn);
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (selfField.AllyHandCards.Count <= playPtn[i])
			{
				AIConsoleUtility.LogError("AIVirtualField GetPlayTokenCount Error!! AllyHandCards out of range!!!!!");
				continue;
			}
			AIVirtualCard aIVirtualCard = selfField.AllyHandCards[playPtn[i]];
			AIVirtualCard actor = aIVirtualCard;
			if (playptnRecordOnSim != null)
			{
				PlayedCardInfo playedCardInfo = playptnRecordOnSim.PlayedCardList[i];
				if (playedCardInfo.TransformCard != null)
				{
					actor = playedCardInfo.TransformCard;
				}
			}
			AIVirtualTargetSelectAction situation2 = new AIVirtualTargetSelectAction(actor, aIVirtualCard, AIOperationType.PLAY, situation?.SelectedTargets);
			List<AITokenInformation> allySideTokenIdsOfPlaySituation = GetAllySideTokenIdsOfPlaySituation(selfField, playPtn, situation2);
			if (allySideTokenIdsOfPlaySituation == null)
			{
				continue;
			}
			List<AIVirtualCard> list = new List<AIVirtualCard>();
			for (int j = 0; j < allySideTokenIdsOfPlaySituation.Count; j++)
			{
				AIVirtualCard tokenFromId = selfField.AI.tokenManager.GetTokenFromId(allySideTokenIdsOfPlaySituation[j].TokenId, aIVirtualCard.IsAlly, selfField);
				if (tokenFromId != null)
				{
					list.Add(tokenFromId);
				}
			}
			list = AIFilteringUtility.MultipleFiltering(list, filters, tagOwner, playPtn, situation);
			if (list.IsNotNullOrEmpty())
			{
				num += allySideTokenIdsOfPlaySituation.Count;
			}
			if (aIVirtualCard.IsSameCard(tagOwner))
			{
				break;
			}
		}
		return num;
	}
}
