using System.Collections.Generic;

namespace Wizard;

public static class AIAttachTagSimulationUtility
{
	public static void SimulateAttachTagToAll(List<AIVirtualCard> targets, AIVirtualCard owner, AIPlayTag tag, AIScriptTokenArgType removeTiming, AISituationInfo situation)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsIndependent)
			{
				SimulateAttachTagToSingle(aIVirtualCard, owner, tag, removeTiming, situation);
			}
		}
	}

	public static void SimulateAttachTagToTarget(AISituationInfo situation, AIVirtualCard owner, AIScriptTokenArgType whichTarget, AIPlayTag tag, AIScriptTokenArgType removeTiming)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsIndependent)
			{
				SimulateAttachTagToSingle(aIVirtualCard, owner, tag, removeTiming, situation);
			}
		}
	}

	public static void SimulateAttachTagToSingle(AIVirtualCard target, AIVirtualCard owner, AIPlayTag tag, AIScriptTokenArgType removeTiming, AISituationInfo situation)
	{
		AIAttachedTagInformation info = new AIAttachedTagInformation(tag, removeTiming, owner, target);
		target.TagCollectionContainer.AttachTag(info, target, situation);
		RegisterAttachedTagStopPreprocess(owner, owner.SelfField, tag, removeTiming, target);
	}

	public static void SimulateRandomSelectAttachTag(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIPlayTag tag, AIScriptTokenArgType removeTiming, AISelectLogicArgumentBase selectLogic)
	{
		AIVirtualCardRealTargetInformation aIVirtualCardRealTargetInformation = situation.DequeueRealTargetInfo(tagOwner, field);
		if (aIVirtualCardRealTargetInformation == null)
		{
			SimulateRandomSelectAttachTagToVirtualTarget(candidates, selectCount, tagOwner, field, playPtn, situation, tag, removeTiming, selectLogic);
		}
		else
		{
			SimulateRandomSelectAttachTagToRealTarget(aIVirtualCardRealTargetInformation, candidates, selectCount, tagOwner, situation, tag, removeTiming);
		}
	}

	private static void SimulateRandomSelectAttachTagToVirtualTarget(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIPlayTag tag, AIScriptTokenArgType removeTiming, AISelectLogicArgumentBase selectLogic)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		if (selectCount >= candidates.Count)
		{
			SimulateAttachTagToAll(candidates, tagOwner, tag, removeTiming, situation);
			return;
		}
		if (selectCount == 1)
		{
			SimulateAttachTagToSingle(selectLogic.SelectSingleTarget(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst), tagOwner, tag, removeTiming, situation);
			return;
		}
		List<AIVirtualCard> list = selectLogic.SelectMultipleSelectedTargets(candidates, selectCount, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst);
		if (list != null && list.Count > 0)
		{
			SimulateAttachTagToAll(list, tagOwner, tag, removeTiming, situation);
		}
	}

	private static void SimulateRandomSelectAttachTagToRealTarget(AIVirtualCardRealTargetInformation realTargetInfo, List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AISituationInfo situation, AIPlayTag tag, AIScriptTokenArgType removeTiming)
	{
		List<AIVirtualCard> targetList = realTargetInfo.TargetList;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		int num = selectCount;
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard item = targetList[i];
			if (candidates.Contains(item))
			{
				list.Add(item);
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		SimulateAttachTagToAll(list, tagOwner, tag, removeTiming, situation);
	}

	private static void RegisterAttachedTagStopPreprocess(AIVirtualCard tagOwner, AIVirtualField field, AIPlayTag attachedTag, AIScriptTokenArgType removeTiming, AIVirtualCard targetCard)
	{
		AIAttachedTagStopPreprocessOption option = new AIAttachedTagStopPreprocessOption(targetCard)
		{
			TargetTag = attachedTag
		};
		switch (GetAttachedTagRemoveTimingForPreprocess(removeTiming, tagOwner))
		{
		case AIAttachedTagRemoveTiming.AllyTurnEnd:
			field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option);
			break;
		case AIAttachedTagRemoveTiming.OpponentTurnEnd:
			field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option);
			break;
		case AIAttachedTagRemoveTiming.NextTurnEnd:
		{
			int defaultDecrementValue = (tagOwner.IsSelfTurn ? 1 : 0);
			if (tagOwner.IsAlly)
			{
				field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option, defaultDecrementValue);
			}
			else
			{
				field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option, defaultDecrementValue);
			}
			break;
		}
		case AIAttachedTagRemoveTiming.AllyTurnStart:
			field.TagPreprocessContainer.AppendAllyTurnStartStopInfo(option);
			break;
		case AIAttachedTagRemoveTiming.OpponentTurnStart:
			field.TagPreprocessContainer.AppendOpponentTurnStartStopInfo(option);
			break;
		case AIAttachedTagRemoveTiming.Leave:
			field.TagPreprocessContainer.AppendLeaveStopInfo(option, tagOwner);
			break;
		}
	}

	private static AIAttachedTagRemoveTiming GetAttachedTagRemoveTimingForPreprocess(AIScriptTokenArgType srcRemoveTiming, AIVirtualCard tagOwner)
	{
		switch (srcRemoveTiming)
		{
		case AIScriptTokenArgType.WHEN_ALLY_TURNEND:
			if (!tagOwner.IsAlly)
			{
				return AIAttachedTagRemoveTiming.OpponentTurnEnd;
			}
			return AIAttachedTagRemoveTiming.AllyTurnEnd;
		case AIScriptTokenArgType.WHEN_OPPONENT_TURNEND:
			if (!tagOwner.IsAlly)
			{
				return AIAttachedTagRemoveTiming.AllyTurnEnd;
			}
			return AIAttachedTagRemoveTiming.OpponentTurnEnd;
		case AIScriptTokenArgType.WHEN_ALLY_TURNSTART:
			if (!tagOwner.IsAlly)
			{
				return AIAttachedTagRemoveTiming.OpponentTurnStart;
			}
			return AIAttachedTagRemoveTiming.AllyTurnStart;
		case AIScriptTokenArgType.WHEN_OPPONENT_TURNSTART:
			if (!tagOwner.IsAlly)
			{
				return AIAttachedTagRemoveTiming.AllyTurnStart;
			}
			return AIAttachedTagRemoveTiming.OpponentTurnStart;
		case AIScriptTokenArgType.WHEN_NEXT_TURNEND:
			return AIAttachedTagRemoveTiming.NextTurnEnd;
		case AIScriptTokenArgType.WHEN_LEAVE:
			return AIAttachedTagRemoveTiming.Leave;
		default:
			return AIAttachedTagRemoveTiming.None;
		}
	}
}
