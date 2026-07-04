using System.Collections.Generic;

namespace Wizard;

public static class AIBounceSimulationUtility
{
	public static void BounceAll(List<AIVirtualCard> candidates, AISituationInfo situation)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (!aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Bounce, isFromSkill: true);
				BounceActivation(aIVirtualCard, situation);
			}
		}
	}

	public static void BounceRandom(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int selectCount = 1)
	{
		if (selectCount <= 1)
		{
			AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectRemovalTarget(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Bounce);
			if (aIVirtualCard != null && !aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Bounce, isFromSkill: true);
				BounceActivation(aIVirtualCard, situation);
			}
		}
		else
		{
			BounceAll(AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Bounce, selectCount), situation);
		}
	}

	public static void ExecuteTargetSelectBounce(AIVirtualCard owner, List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType selectType, AIRemovalType removalType, int selectCount = 1)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectDestroy() Error!! situation is null!!!!!");
			return;
		}
		if (situation.IsTargetExists(selectType))
		{
			BounceTarget(situation, targets, selectType);
			return;
		}
		situation.SelectedTargets.UpdateRemovalType(selectType, removalType);
		BounceTargetPrediction(situation, targets, owner, field, playPtn, selectType, selectCount);
	}

	private static void BounceTargetPrediction(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard bounceOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType selectType, int selectCount)
	{
		List<AIVirtualCard> candidates2 = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, bounceOwner, playPtn);
		if (selectCount <= 1)
		{
			AIVirtualCard target = AISimulationRemovalUtility.SelectRemovalTarget(candidates2, bounceOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Bounce);
			situation.SetSingleTargetInInfo(target, TargetSelectType.Default, selectType);
			BounceTarget(situation, candidates, selectType);
		}
		else
		{
			List<AIVirtualCard> list = AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates2, bounceOwner, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Bounce, selectCount);
			situation.SetMultipleTargetsInInfo(list, TargetSelectType.Default, AIRemovalType.Bounce, selectType);
			BounceAll(list, situation);
		}
	}

	public static void BounceTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("BounceTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard) && !aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Bounce, isFromSkill: true);
			}
			BounceActivation(aIVirtualCard, situation);
		}
	}

	private static void BounceActivation(AIVirtualCard bouncedCard, AISituationInfo situation)
	{
		AIVirtualField selfField = situation.Actor.SelfField;
		if (!selfField.CardListSet.HasBounceTagHolders)
		{
			return;
		}
		List<AIVirtualCard> bounceTagHolders = selfField.CardListSet.BounceTagHolders;
		for (int i = 0; i < bounceTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = bounceTagHolders[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsOnField)
			{
				aIVirtualCard.ExecuteBounceSkills(bouncedCard, selfField, selfField.BestPlayPtn, situation);
			}
		}
	}

	public static int GetBounceCount(AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType type)
	{
		int result = 0;
		switch (type)
		{
		case AIScriptTokenArgType.NOW:
			result = GetCurrentSituationBounceCount(situation);
			break;
		case AIScriptTokenArgType.TURN:
			result = field.TurnBounceCount + GetCurrentSituationBounceCount(situation);
			break;
		case AIScriptTokenArgType.GAME:
			result = field.GameBounceCount + GetCurrentSituationBounceCount(situation);
			break;
		}
		return result;
	}

	private static int GetCurrentSituationBounceCount(AISituationInfo situation)
	{
		if (situation.BounceCardList == null)
		{
			return 0;
		}
		return situation.BounceCardList.Count;
	}
}
