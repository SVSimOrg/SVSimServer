using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public static class AIBestFusionPatternCalculator
{
	public static AIFusionSituationInfo CalculateBestFusionPattern(AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		AIFusionSituationInfo aIFusionSituationInfo = null;
		float num = 0f;
		List<int> playPtn = playPtnRecord.PlayPtn;
		List<float> collection = CalculateAllHandBonus(field, playPtnRecord);
		float num2 = 0f;
		if (playPtn == null || playPtn.Count <= 0)
		{
			num2 -= CalculateHandOverflowPenalty(field, null, null);
		}
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[i];
			if (!aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.Fusion) || !aIVirtualCard.IsFusionable)
			{
				continue;
			}
			AIFusionSituationInfo aIFusionSituationInfo2 = CreateFusionSituation(aIVirtualCard, field, playPtn);
			float value = CalculateHandBonusForFusion(aIVirtualCard, field, playPtnRecord, aIFusionSituationInfo2, isIgnoreInFusion: true);
			List<float> list = new List<float>(collection);
			list[i] = value;
			float num3 = list.Sum() + num2;
			List<AIVirtualCard> fusionCandidates = GetFusionCandidates(aIVirtualCard, field, aIFusionSituationInfo2, playPtn);
			if (fusionCandidates == null || fusionCandidates.Count <= 0)
			{
				continue;
			}
			int count = fusionCandidates.Count;
			int num4 = (int)Mathf.Pow(2f, count);
			AISelectedTargetInfo info = null;
			bool flag = false;
			for (int j = 1; j < num4; j++)
			{
				AISelectedTargetInfo aISelectedTargetInfo = new AISelectedTargetInfo(ConvertPatternIndexToList(fusionCandidates, j), TargetSelectType.NormalRuleBase);
				aIFusionSituationInfo2.SetTarget(aISelectedTargetInfo, AIScriptTokenArgType.TARGET_SELECT);
				float num5 = CalculateAfterFusionHandValue(aIFusionSituationInfo2, field, playPtnRecord, list) - num3;
				if (num5 > num)
				{
					flag = true;
					num = num5;
					info = aISelectedTargetInfo;
				}
			}
			if (flag)
			{
				aIFusionSituationInfo = aIFusionSituationInfo2;
				aIFusionSituationInfo.SetTarget(info, AIScriptTokenArgType.TARGET_SELECT);
			}
		}
		return aIFusionSituationInfo;
	}

	private static AIFusionSituationInfo CreateFusionSituation(AIVirtualCard actor, AIVirtualField field, List<int> playPtn)
	{
		AIFusionSituationInfo aIFusionSituationInfo = new AIFusionSituationInfo(actor, null);
		if (!aIFusionSituationInfo.InitializeFusionParameter(field, playPtn))
		{
			return null;
		}
		return aIFusionSituationInfo;
	}

	private static List<AIVirtualCard> GetFusionCandidates(AIVirtualCard actor, AIVirtualField field, AIFusionSituationInfo fusion, List<int> playPtn)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.AllyHandCards, fusion.Range, actor, playPtn, fusion);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		list.Remove(actor);
		return list;
	}

	private static List<AIVirtualCard> ConvertPatternIndexToList(List<AIVirtualCard> candidates, int patternIndex)
	{
		int num = patternIndex;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		int num2 = 0;
		while (num > 0)
		{
			bool flag = num % 2 > 0;
			num /= 2;
			if (flag)
			{
				list.Add(candidates[num2]);
			}
			num2++;
			if (num2 >= candidates.Count)
			{
				break;
			}
		}
		return list;
	}

	private static List<float> CalculateAllHandBonus(AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			float item = CalculateHandBonusForFusion(field.AllyHandCards[i], field, playPtnRecord, null, isIgnoreInFusion: false);
			list.Add(item);
		}
		return list;
	}

	private static float CalculateAfterFusionHandValue(AIFusionSituationInfo fusion, AIVirtualField field, AISinglePlayptnRecord playPtnRecord, List<float> allHandBonusList)
	{
		float num = 0f;
		List<int> playPtn = playPtnRecord.PlayPtn;
		List<AIVirtualCard> targets = fusion.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT).Targets;
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[i];
			if (!targets.Contains(aIVirtualCard))
			{
				num = ((!aIVirtualCard.IsSameCard(fusion.Actor)) ? (num + allHandBonusList[i]) : (num + CalculateHandBonusForFusion(aIVirtualCard, field, playPtnRecord, fusion, isIgnoreInFusion: true)));
			}
		}
		num += fusion.Actor.GetFusionBonus(playPtn, fusion);
		if (playPtn == null || playPtn.Count <= 0)
		{
			num -= CalculateHandOverflowPenalty(field, targets, fusion);
		}
		return num;
	}

	private static float CalculateHandBonusForFusion(AIVirtualCard card, AIVirtualField field, AISinglePlayptnRecord playPtnRecord, AIFusionSituationInfo fusion, bool isIgnoreInFusion)
	{
		List<int> playPtn = playPtnRecord.PlayPtn;
		float num = card.GetHandBonus(playPtn, fusion, isIgnoreInFusion);
		bool flag = false;
		float num2 = card.Cost;
		int num3 = field.AllyPp;
		for (int i = 0; i < playPtnRecord.PlayedCardList.Count; i++)
		{
			PlayedCardInfo playedCardInfo = playPtnRecord.PlayedCardList[i];
			if (playedCardInfo.Card.IsSameCard(card))
			{
				flag = true;
				num2 = num3 - playedCardInfo.RestPp;
				break;
			}
			num3 = playedCardInfo.RestPp;
		}
		if (flag)
		{
			num = 2f * (num + 2f);
		}
		return num - Mathf.Abs(num2 - (float)field.AllyPpTotal) * 0.01f;
	}

	private static float CalculateHandOverflowPenalty(AIVirtualField field, List<AIVirtualCard> fusionCandidates, AISituationInfo fusionSituation)
	{
		float num = 0f;
		int num2 = field.AllyHandCards.Count;
		if (fusionCandidates != null && fusionSituation != null && fusionSituation.ActionType == AIOperationType.FUSION)
		{
			num2 -= fusionCandidates.Count;
			int fusionDrawCount = fusionSituation.Actor.GetFusionDrawCount(EnemyAI.EmptyPlayPtn, fusionSituation);
			num2 += fusionDrawCount;
			if (num2 > 9)
			{
				num += 2f * (float)(num2 - 9);
				num2 = 9;
			}
		}
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard tagOwner = field.AllyInplayCards[i];
			num2 += tagOwner.GetHandPlusCount(EnemyAI.EmptyPlayPtn);
		}
		if (num2 > 8)
		{
			num += 2f * (float)(num2 - 8);
		}
		return num;
	}
}
