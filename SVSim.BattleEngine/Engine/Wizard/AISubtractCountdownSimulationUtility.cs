using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public static class AISubtractCountdownSimulationUtility
{
	public static void SubtractCountdownAll(List<AIVirtualCard> targetCards, int value, AISituationInfo situation)
	{
		for (int i = 0; i < targetCards.Count; i++)
		{
			SubtractCountdownSingle(targetCards[i], value, situation);
		}
	}

	public static void ExecuteTargetSelectSubtractCountdown(AIVirtualCard tagOwner, List<AIVirtualCard> candidats, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType selectType, int countChange)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectSubtractCountdown() Error!! situation is null!!!!!");
		}
		else if (situation.IsTargetExists(selectType))
		{
			SubtractCountdownTarget(situation, candidats, selectType, countChange);
		}
		else
		{
			SubtractCountdownTargetPrediction(situation, candidats, tagOwner, field, playPtn, selectType, countChange);
		}
	}

	private static void SubtractCountdownTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget, int value)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("SubtractCountdownTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				SubtractCountdownSingle(aIVirtualCard, value, situation);
			}
		}
	}

	private static void SubtractCountdownTargetPrediction(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType whichTarget, int countChange)
	{
		AIVirtualCard target = SelectBestTarget(AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, owner, playPtn), countChange);
		situation.SetSingleTargetInInfo(target, TargetSelectType.Default, whichTarget);
		SubtractCountdownTarget(situation, candidates, whichTarget, countChange);
	}

	public static void SubtractCountdownSingle(AIVirtualCard target, int value, AISituationInfo situation)
	{
		if (!target.IsDead && target.IsCountdownAmulet)
		{
			target.ChantCountDown(situation, value);
		}
	}

	private static AIVirtualCard SelectBestTarget(List<AIVirtualCard> candidates, int countChange)
	{
		AIVirtualCard result = null;
		float num = float.MinValue;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			float num2 = aIVirtualCard.EvaluateBreakValue(emptyPlayPtn, useIgnoreBreak: false);
			int num3 = Mathf.Max(0, aIVirtualCard.ChantCount - countChange);
			float num4 = num2 * Mathf.Pow(0.75f, num3);
			if (num4 > num)
			{
				result = aIVirtualCard;
				num = num4;
			}
		}
		return result;
	}
}
