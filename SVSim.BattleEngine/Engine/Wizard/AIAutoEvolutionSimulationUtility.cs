using System.Collections.Generic;

namespace Wizard;

public static class AIAutoEvolutionSimulationUtility
{
	public static void AutoEvolution(AIVirtualField field, List<AIVirtualCard> targets, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType selecyType)
	{
		switch (selecyType)
		{
		case AIScriptTokenArgType.RANDOM_SELECT:
			AutoEvolveRandom(targets, field, playPtn, situation);
			break;
		case AIScriptTokenArgType.ALL_SELECT:
			AutoEvolveAll(targets, field, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			AutoEvolveTarget(situation, field, targets, selecyType);
			break;
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			break;
		}
	}

	private static void AutoEvolveRandom(List<AIVirtualCard> targets, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIVirtualCard aIVirtualCard = targets.FindMin((AIVirtualCard c) => c.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) * (float)(c.IsAlly ? 1 : (-1)));
		if (aIVirtualCard != null)
		{
			AutoEvolveSingle(aIVirtualCard, field, situation);
		}
	}

	private static void AutoEvolveAll(List<AIVirtualCard> targets, AIVirtualField field, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!aIVirtualCard.IsEvolution && !aIVirtualCard.IsDead)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list != null && list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				AIVirtualEvolutionSimulator.AutoEvolve(list[j], field, situation);
			}
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.EvoActivateCount, list);
		}
	}

	private static void AutoEvolveTarget(AISituationInfo situation, AIVirtualField field, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard) && !aIVirtualCard.IsEvolution)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list != null && list.Count > 0)
		{
			AutoEvolveAll(list, field, situation);
		}
	}

	public static void AutoEvolveSingle(AIVirtualCard target, AIVirtualField field, AISituationInfo situation)
	{
		if (!target.IsEvolution && !target.IsDead)
		{
			AIVirtualEvolutionSimulator.AutoEvolve(target, field, situation);
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.EvoActivateCount, target);
		}
	}
}
