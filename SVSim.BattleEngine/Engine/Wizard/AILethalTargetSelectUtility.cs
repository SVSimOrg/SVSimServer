using System.Collections.Generic;

namespace Wizard;

public static class AILethalTargetSelectUtility
{
	public class LethalTargetSelectResult
	{
		public AISelectedTargetInfoSet TargetSet;

		public AIVirtualField ResultField;
	}

	public static bool LethalSelectTarget(AIVirtualField field, AIVirtualTargetSelectAction situation, AISinglePlayptnRecord playPtnRecord)
	{
		AIVirtualCard actor = situation.Actor;
		List<AIVirtualTargetSelectInfo> list = actor.CreateAIVirtualSelectInfo(field, situation);
		if (list == null || list.Count <= 0)
		{
			return TargetSelectAction(field, situation);
		}
		AISelectedTargetInfoSet preDecidedTargetSet = null;
		if (situation.SelectedTargets.IsAnyTargetExists())
		{
			preDecidedTargetSet = situation.SelectedTargets;
		}
		List<AISelectedTargetInfoSet> allTargetSelectSimulationPattern = AIVirtualTargetSelectSimulator.GetAllTargetSelectSimulationPattern(list, preDecidedTargetSet, situation, field, playPtnRecord);
		if (allTargetSelectSimulationPattern == null || allTargetSelectSimulationPattern.Count <= 0)
		{
			AIConsoleUtility.LogError("LethalSelectTarget error!! Cannot find target pattern!!!!! actor.id == " + actor.BaseId);
			return TargetSelectAction(field, situation);
		}
		LethalTargetSelectResult lethalTargetSelectResult = new LethalTargetSelectResult
		{
			ResultField = field,
			TargetSet = null
		};
		CalculateBestTargetPattern(lethalTargetSelectResult, field, situation, allTargetSelectSimulationPattern, playPtnRecord, list);
		situation.SelectedTargets = lethalTargetSelectResult.TargetSet;
		return TargetSelectAction(field, situation);
	}

	private static bool TargetSelectAction(AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		switch (situation.ActionType)
		{
		case AIOperationType.PLAY:
		{
			PlaySimulationInfo playSimulationInfo = AIPlayCardSimulationUtility.CreatePlaySimulationInfo(situation.Actor, situation, field);
			if (playSimulationInfo != null)
			{
				AIVirtualPlaySimulator.PlayCard(situation, field, playSimulationInfo);
				return true;
			}
			return false;
		}
		case AIOperationType.EVOLVE:
			AIVirtualEvolutionSimulator.ManualEvolve(situation, field);
			return true;
		default:
			return false;
		}
	}

	private static void CalculateBestTargetPattern(LethalTargetSelectResult result, AIVirtualField field, AIVirtualTargetSelectAction situation, List<AISelectedTargetInfoSet> patternSet, AISinglePlayptnRecord playptnRecord, List<AIVirtualTargetSelectInfo> selectInfoList)
	{
		AIVirtualCard originalCard = situation.OriginalCard;
		AIOperationType actionType = situation.ActionType;
		int giveQuickSelectIndex = GetGiveQuickSelectIndex(selectInfoList);
		bool isSecondTargetForbbidenSelectedTarget = selectInfoList.Count > 1 && selectInfoList[1].IsForbiddenSelectedTarget;
		for (int i = 0; i < patternSet.Count; i++)
		{
			AISelectedTargetInfoSet aISelectedTargetInfoSet = patternSet[i];
			if (!AIVirtualTargetSelectSimulator.IsAbleToReplaceDummyTarget(aISelectedTargetInfoSet, result.TargetSet, isSecondTargetForbbidenSelectedTarget))
			{
				continue;
			}
			if (!aISelectedTargetInfoSet.IsTargetExist(0) && aISelectedTargetInfoSet.IsTargetExist(1))
			{
				AIConsoleUtility.LogError("AILethalTargetSelectUtility.CalculateBestTargetPattern error!! targetPattern.firstTarget is null & secondTaget is not null");
				continue;
			}
			AIVirtualField aIVirtualField = new AIVirtualField(field);
			AISelectedTargetInfoSet similarTargetInfoSet = aISelectedTargetInfoSet.GetSimilarTargetInfoSet(aIVirtualField);
			AIVirtualCard aIVirtualCard = aIVirtualField.SearchVirtualCard(originalCard);
			AIVirtualCard actor = aIVirtualCard;
			if (actionType == AIOperationType.PLAY)
			{
				actor = aIVirtualCard.FindRealActor(playptnRecord);
			}
			AIVirtualTargetSelectAction situation2 = new AIVirtualTargetSelectAction(actor, aIVirtualCard, actionType, similarTargetInfoSet);
			TargetSelectAction(aIVirtualField, situation2);
			if (giveQuickSelectIndex >= 0)
			{
				GiveQuickTargetSelectSimulation(similarTargetInfoSet.Get(giveQuickSelectIndex), aIVirtualField);
			}
			if (result.TargetSet == null || aIVirtualField.EnemyClass.Life < result.ResultField.EnemyClass.Life)
			{
				result.ResultField = aIVirtualField;
				result.TargetSet = aISelectedTargetInfoSet.Clone();
			}
		}
	}

	private static int GetGiveQuickSelectIndex(List<AIVirtualTargetSelectInfo> selectInfoList)
	{
		for (int i = 0; i < selectInfoList.Count; i++)
		{
			AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = selectInfoList[i];
			if (aIVirtualTargetSelectInfo.SelectRule != null && (aIVirtualTargetSelectInfo.SelectRule.Type == AIPlayTagType.PlayQuick || aIVirtualTargetSelectInfo.SelectRule.Type == AIPlayTagType.FanfareQuick))
			{
				return i;
			}
		}
		return -1;
	}

	private static void GiveQuickTargetSelectSimulation(AISelectedTargetInfo receiveQuickTargets, AIVirtualField field)
	{
		if (!receiveQuickTargets.HasTarget)
		{
			return;
		}
		for (int i = 0; i < receiveQuickTargets.Targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = receiveQuickTargets.Targets[i];
			AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(aIVirtualCard, field.EnemyClass);
			if (AIAttackSimulationUtility.IsAttackPossible(field, aIVirtualAttackInfo) && !aIVirtualAttackInfo.WillTargetDestroyByAttackTags(field, field.BestPlayPtn, aIVirtualCard))
			{
				AIVirtualAttackSimulator.Attack(aIVirtualAttackInfo, field);
			}
		}
	}
}
