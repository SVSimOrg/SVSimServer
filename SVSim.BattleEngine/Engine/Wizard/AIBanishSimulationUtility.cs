using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIBanishSimulationUtility
{
	public static float EvalTargetingBanish(List<AIScriptTokenBase> argList, List<int> playPtn, AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, argList, tagOwner, playPtn, null);
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
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsUnbanishable && (aIVirtualCard.IsAlly == tagOwner.IsAlly || (!aIVirtualCard.IsUntouchable && !aIVirtualCard.IsSneak)))
			{
				num++;
				float num3 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true);
				float num4 = (aIVirtualCard.IsAlly ? (0f - num3 - aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + aIVirtualCard.GetAllBanishBonus(playPtn, useIgnoreInBattle: false) + aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false)) : (num3 + aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - aIVirtualCard.GetAllBanishBonus(playPtn, useIgnoreInBattle: false) - aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false)));
				if (num4 > num2)
				{
					num2 = num4;
				}
			}
		}
		if (num == 0)
		{
			num2 = 0f;
		}
		return num2;
	}

	public static float EvalRandomBanish(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, int count, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, argList, tagOwner, playPtn, null);
		if (list != null && list.Count > 0)
		{
			List<float> list2 = new List<float>();
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (!aIVirtualCard.IsUnbanishable && !aIVirtualCard.IsIndependent)
				{
					float num2 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
					float num3 = aIVirtualCard.EvaluateBreakValue(playPtn, useIgnoreBreak: true);
					float allBanishBonus = aIVirtualCard.GetAllBanishBonus(playPtn, useIgnoreInBattle: true);
					float num4 = aIVirtualCard.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
					float num5 = (aIVirtualCard.IsAlly ? (-1f) : 1f) * (num2 + num3 - allBanishBonus - num4);
					list2.Add(num5);
					num += num5;
				}
			}
			if (list.Count <= count)
			{
				return num;
			}
			List<float[]> list3 = AIMathematicsLibrary.EnumerateCombinations(list2, count).ToList();
			float num6 = 0f;
			for (int j = 0; j < list3.Count; j++)
			{
				for (int k = 0; k < list3[j].Length; k++)
				{
					num6 += list3[j][k];
				}
			}
			return num6 / (float)list3.Count;
		}
		return 0f;
	}

	public static float EvalAllBanish(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, argList, tagOwner, playPtn, null);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsUnbanishable)
			{
				float num2 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
				float num3 = aIVirtualCard.EvaluateBreakValue(playPtn, useIgnoreBreak: true);
				float allBanishBonus = aIVirtualCard.GetAllBanishBonus(playPtn, useIgnoreInBattle: true);
				float num4 = aIVirtualCard.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
				num += (aIVirtualCard.IsAlly ? (-1f) : 1f) * (num2 + num3 - allBanishBonus - num4);
			}
		}
		return num;
	}

	public static void BanishAll(List<AIVirtualCard> targetCards, AISituationInfo situation)
	{
		for (int i = 0; i < targetCards.Count; i++)
		{
			BanishSingle(targetCards[i], situation);
		}
	}

	public static void ExecuteTargetSelectBanish(AIVirtualCard owner, List<AIVirtualCard> targets, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType selectType, int selectCount = 1)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectBanish() Error!! situation is null!!!!!");
		}
		else if (situation.IsTargetExists(selectType))
		{
			BanishTarget(situation, targets, selectType);
		}
		else
		{
			BanishTargetPrediction(situation, targets, owner, field, playPtn, selectType, selectCount);
		}
	}

	private static void BanishTargetPrediction(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard banishOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType selectType, int selectCount)
	{
		List<AIVirtualCard> candidates2 = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, banishOwner, playPtn);
		if (selectCount <= 1)
		{
			AIVirtualCard target = AISimulationRemovalUtility.SelectRemovalTarget(candidates2, banishOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Banish);
			situation.SetSingleTargetInInfo(target, TargetSelectType.Default, selectType);
			BanishTarget(situation, candidates, selectType);
		}
		else
		{
			BanishAll(AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates2, banishOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Banish, selectCount), situation);
		}
	}

	public static void BanishTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("BanishTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				BanishSingle(aIVirtualCard, situation);
			}
		}
	}

	public static void BanishSingle(AIVirtualCard target, AISituationInfo situation)
	{
		if (!target.IsDead && !target.IsIndependent && !target.IsUnbanishable)
		{
			target.RemoveCard(situation, AIRemovalType.Banish, isFromSkill: true);
		}
	}

	public static void BanishRandom(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int selectCount = 1)
	{
		if (selectCount <= 1)
		{
			AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectRemovalTarget(targets, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Banish);
			if (aIVirtualCard != null)
			{
				BanishSingle(aIVirtualCard, situation);
			}
		}
		else
		{
			BanishAll(AISimulationRemovalUtility.SelectMultipleRemovalTargets(targets, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Banish, selectCount), situation);
		}
	}

	public static int GetInplayBanishedCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		return GetFilterdBanishCardList(tagOwner, field, playPtn, situation, argList, BattleCardBase.BanishInfo.BanishPlace.Field)?.Count ?? 0;
	}

	public static int GetHandBanishedCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		return GetFilterdBanishCardList(tagOwner, field, playPtn, situation, argList, BattleCardBase.BanishInfo.BanishPlace.Hand)?.Count ?? 0;
	}

	private static List<AIVirtualCard> GetFilterdBanishCardList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList, BattleCardBase.BanishInfo.BanishPlace place)
	{
		List<AIVirtualCard> banishedCards = field.CardListSet.BanishedCards;
		if (banishedCards == null || banishedCards.Count <= 0)
		{
			return null;
		}
		SeparateArgListToFilterAndTimingArg(argList, out var filters, out var turnOrGame);
		if (filters == null || filters.Count <= 0 || (turnOrGame != AIScriptTokenArgType.TURN && turnOrGame != AIScriptTokenArgType.GAME))
		{
			return null;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(banishedCards, filters, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		if (turnOrGame == AIScriptTokenArgType.TURN)
		{
			list.RemoveAll((AIVirtualCard c) => !c.IsBanishedTargetTurn(field.CurrentTurnCount));
		}
		list.RemoveAll((AIVirtualCard c) => c.BanishedInfo.Place != place);
		return list;
	}

	private static void SeparateArgListToFilterAndTimingArg(List<AIScriptTokenBase> argList, out List<AIScriptTokenBase> filters, out AIScriptTokenArgType turnOrGame)
	{
		if (argList == null || argList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIBanishSimulationUtility.SeparateArgListToFilterAndTimingArg() error!! argList is null!!");
			filters = null;
			turnOrGame = AIScriptTokenArgType.NONE;
			return;
		}
		if (argList[argList.Count - 1] is AIScriptArgumentToken aIScriptArgumentToken)
		{
			turnOrGame = aIScriptArgumentToken.ArgumentType;
		}
		else
		{
			AIConsoleUtility.LogError("AIBanishSimulationUtility.SeparateArgListToFilterAndTimingArg() error!! lastToken is not ArgumentToken!!");
			turnOrGame = AIScriptTokenArgType.NONE;
		}
		filters = argList.GetRange(0, argList.Count - 1);
	}
}
