using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIDestroySimulationUtility
{
	public static float CalcEvalDestroy(AIVirtualCard card, List<int> playPtn, AISituationInfo situation, bool useIgnoreBreak)
	{
		return (card.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreBreak) - card.GetAllBreakBonus(playPtn, useIgnoreBreak) - card.GetAllLeaveBonus(playPtn, useIgnoreBreak)) * (card.IsAlly ? (-1f) : 1f);
	}

	public static float EvalTargetingDestroy(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, tagOwner, playPtn);
		int num = 0;
		float num2 = float.MinValue;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsAlly == tagOwner.IsAlly || (!aIVirtualCard.IsUntouchable && !aIVirtualCard.IsSneak))
			{
				num++;
				float num3 = 0f;
				if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
				{
					num3 = CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false);
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
		}
		if (num == 0)
		{
			num2 = 0f;
		}
		return num2;
	}

	public static float EvalTargetingOtherDestroy(List<AIScriptTokenBase> target_filters, List<AIScriptTokenBase> destroy_filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, int select_count)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(selfField.CardListSet.BothInplayCards, target_filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		List<AIVirtualCard> list2 = null;
		List<float> list3 = new List<float>();
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsUntouchable || aIVirtualCard.IsSneak)
			{
				if (list2 == null)
				{
					list2 = new List<AIVirtualCard>();
				}
				list2.Add(aIVirtualCard);
				bool flag = AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, selfField.AllyHandCards, destroy_filters, playPtn, tagOwner, situation);
				if (aIVirtualCard.IsIndependent || aIVirtualCard.IsIndestructible || !flag)
				{
					list3.Add(0f);
					continue;
				}
				float item = CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false);
				list3.Add(item);
			}
		}
		if (list2 != null && list.Count - list2.Count < select_count)
		{
			return 0f;
		}
		List<AIVirtualCard> list4 = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, tagOwner, playPtn);
		List<float> list5 = null;
		for (int j = 0; j < list4.Count; j++)
		{
			if (list5 == null)
			{
				list5 = new List<float>();
			}
			AIVirtualCard aIVirtualCard2 = list4[j];
			bool flag2 = AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard2, selfField.AllyHandCards, destroy_filters, playPtn, tagOwner, situation);
			if (aIVirtualCard2.IsIndependent || aIVirtualCard2.IsIndestructible || !flag2)
			{
				list5.Add(0f);
				continue;
			}
			float item2 = CalcEvalDestroy(aIVirtualCard2, playPtn, situation, useIgnoreBreak: false);
			list5.Add(item2);
		}
		list5?.Sort((float score0, float score1) => (int)(score1 - score0));
		List<AIVirtualCard> list6 = new List<AIVirtualCard>();
		for (int num = 0; num < list.Count; num++)
		{
			int num2 = 0;
			for (num2 = 0; num2 < list4.Count; num2++)
			{
				if (list[num].Equals(list4[num2]))
				{
					num2 = 99999999;
					break;
				}
			}
			int num3 = 0;
			num3 = 0;
			while (list2 != null && num3 < list2.Count)
			{
				if (list[num].Equals(list2[num3]))
				{
					num3 = 99999999;
					break;
				}
				num3++;
			}
			if (num2 != 99999999 && num3 != 99999999)
			{
				list6.Add(list[num]);
			}
		}
		List<float> list7 = null;
		if (list6.Count > 0)
		{
			list7 = new List<float>();
			for (int num4 = 0; num4 < list6.Count; num4++)
			{
				AIVirtualCard aIVirtualCard3 = list6[num4];
				bool flag3 = AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard3, selfField.AllyHandCards, destroy_filters, playPtn, tagOwner, situation);
				if (aIVirtualCard3.IsIndependent || aIVirtualCard3.IsIndestructible || !flag3)
				{
					list7.Add(0f);
					continue;
				}
				float item3 = CalcEvalDestroy(aIVirtualCard3, playPtn, situation, useIgnoreBreak: false);
				list7.Add(item3);
			}
			list7.Sort((float score0, float score1) => (int)(score1 - score0));
		}
		List<float> list8 = new List<float>();
		if (list3 != null)
		{
			list8.AddRange(list3);
		}
		if (list7 != null)
		{
			list8.AddRange(list7);
		}
		if (list5 != null)
		{
			list8.AddRange(list5);
		}
		int num5 = list.Count - select_count;
		if (num5 >= list8.Count)
		{
			return 0f;
		}
		float num6 = 0f;
		for (int num7 = 0; num7 < num5; num7++)
		{
			num6 += list8[num7];
		}
		return num6;
	}

	public static float EvalTargetingMultiDestroy(List<AIScriptTokenBase> filter, int selectableCount, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothInplayCards, filter, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list.RemoveAll((AIVirtualCard c) => c.IsAlly != tagOwner.IsAlly && (c.IsUntouchable || c.IsSneak));
		if (list.Count <= selectableCount)
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				AIVirtualCard aIVirtualCard = list[num2];
				if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
				{
					float num3 = CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false);
					num += num3;
				}
			}
			return num;
		}
		List<AIVirtualCard> list2 = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, tagOwner, playPtn);
		if (list2.Count <= selectableCount)
		{
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				AIVirtualCard aIVirtualCard2 = list2[num4];
				num += CalcEvalDestroy(aIVirtualCard2, playPtn, situation, useIgnoreBreak: false);
				list.Remove(aIVirtualCard2);
				selectableCount--;
				if (selectableCount <= 0)
				{
					return num;
				}
			}
		}
		else
		{
			list = list2;
		}
		Dictionary<AIVirtualCard, float> dictionary = new Dictionary<AIVirtualCard, float>();
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			AIVirtualCard aIVirtualCard3 = list[num5];
			dictionary.Add(aIVirtualCard3, CalcEvalDestroy(aIVirtualCard3, playPtn, situation, useIgnoreBreak: false));
		}
		for (int num6 = 0; num6 < selectableCount; num6++)
		{
			float num7 = float.MinValue;
			AIVirtualCard aIVirtualCard4 = null;
			for (int num8 = 0; num8 < list.Count; num8++)
			{
				AIVirtualCard aIVirtualCard5 = list[num8];
				float num9 = dictionary[aIVirtualCard5];
				if (num9 > num7)
				{
					num7 = num9;
					aIVirtualCard4 = aIVirtualCard5;
				}
			}
			num += num7;
			if (aIVirtualCard4 != null)
			{
				list.Remove(aIVirtualCard4);
				dictionary.Remove(aIVirtualCard4);
			}
		}
		return num;
	}

	public static float EvalAllDestroy(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothInplayCards, argList, tagOwner, playPtn, situation);
		if (list == null || list.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
			{
				float num2 = CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false);
				num += num2;
			}
		}
		return num;
	}

	public static float EvalRandomDestroy(List<AIScriptTokenBase> argList, int count, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		if (count == 0)
		{
			return 0f;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(selfField.CardListSet.BothInplayCards, argList, tagOwner, playPtn, situation);
		if (list == null || list.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		if (list.Count <= count)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
				{
					float num2 = CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false);
					num += num2;
				}
			}
			return num;
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < list.Count; j++)
		{
			list2.Add(j);
		}
		List<int[]> list3 = AIMathematicsLibrary.EnumerateCombinations(list2, count).ToList();
		for (int k = 0; k < list3.Count; k++)
		{
			int[] array = list3[k];
			for (int l = 0; l < array.Length; l++)
			{
				AIVirtualCard aIVirtualCard2 = list[array[l]];
				if (!aIVirtualCard2.IsIndependent && !aIVirtualCard2.IsIndestructible)
				{
					float num3 = CalcEvalDestroy(aIVirtualCard2, playPtn, situation, useIgnoreBreak: false);
					num += num3;
				}
			}
		}
		return num / (float)list3.Count;
	}

	public static void ExecuteTargetSelectDestroy(AIVirtualCard owner, List<AIVirtualCard> targets, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType selectType, int selectCount = 1)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectDestroy() Error!! situation is null!!!!!");
		}
		else if (situation.IsTargetExists(selectType))
		{
			AISkillSimulationUtility.DestroyTarget(situation, targets, selectType);
		}
		else
		{
			DestroyTargetPrediction(owner, targets, field, playPtn, situation, selectCount, selectType);
		}
	}

	private static void DestroyTargetPrediction(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int selectCount, AIScriptTokenArgType whichTarget)
	{
		AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, tagOwner, playPtn);
		if (selectCount <= 1)
		{
			AIVirtualCard target = AISimulationRemovalUtility.SelectRemovalTarget(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Destroy);
			situation.SetSingleTargetInInfo(target, TargetSelectType.Default, whichTarget);
			AISkillSimulationUtility.DestroyTarget(situation, candidates, whichTarget);
		}
		else
		{
			List<AIVirtualCard> targets = AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Destroy, selectCount);
			situation.SetMultipleTargetsInInfo(targets, TargetSelectType.Default, AIRemovalType.Destroy, whichTarget);
			AISkillSimulationUtility.DestroyTarget(situation, candidates, whichTarget);
		}
	}
}
