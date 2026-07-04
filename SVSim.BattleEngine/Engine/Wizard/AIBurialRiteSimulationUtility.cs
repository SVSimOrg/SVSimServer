using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public static class AIBurialRiteSimulationUtility
{
	public static void ExecuteBurialRite(AIVirtualField field, AISituationInfo situation, AISelectedTargetInfo burialTargetInfo)
	{
		List<AIVirtualCard> targets = burialTargetInfo.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].RemoveAllSkills(situation);
		}
		AISummonTokenUtility.ExecuteSummonCardAll(situation.Actor, field, targets, situation);
		AISkillSimulationUtility.DestroyAll(targets, field, situation);
		if (situation.Actor.IsAlly)
		{
			for (int j = 0; j < targets.Count; j++)
			{
				field.CardListSet.AddAllyBurialCard(targets[j]);
			}
		}
		else
		{
			for (int k = 0; k < targets.Count; k++)
			{
				field.CardListSet.AddEnemyBurialCard(targets[k]);
			}
		}
	}

	public static AIVirtualTargetSelectInfo GetBurialSelectInfo(this AIVirtualCard card, AIVirtualField field, AISituationInfo situation)
	{
		int totalBurialCount = situation.PreprocessRecorder.TotalBurialCount;
		if (totalBurialCount <= 0)
		{
			return null;
		}
		if (card.IsAlly)
		{
			return GetAllyBurialSelectInfo(card, field, situation, totalBurialCount);
		}
		return GetOpponentBurialSelectInfo(card, field, situation, totalBurialCount);
	}

	private static AIVirtualTargetSelectInfo GetAllyBurialSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, int burialCount)
	{
		if (5 - field.AllyInplayCards.Count < burialCount)
		{
			return null;
		}
		List<AIVirtualCard> burialSelectableCards = GetBurialSelectableCards(field.AllyHandCards, owner, situation.OriginalCard);
		if (burialSelectableCards == null || burialSelectableCards.Count < burialCount)
		{
			return null;
		}
		burialSelectableCards = AITargetSelectFilteringUtility.ExecuteTargetFilteringTags(owner, burialSelectableCards, field.BestPlayPtn, situation, burialCount);
		return new AIVirtualTargetSelectInfo(burialCount, burialSelectableCards, TargetSelectType.BurialRite, isForbiddenSelectedTarget: true);
	}

	private static AIVirtualTargetSelectInfo GetOpponentBurialSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, int burialCount)
	{
		List<AIVirtualCard> burialSelectableCards = GetBurialSelectableCards(field.GetEnemyHandCardList(), owner, situation.OriginalCard);
		if (burialSelectableCards == null || burialSelectableCards.Count < burialCount)
		{
			AIConsoleUtility.LogError("GetOpponentBurialSelectInfo() error!! Cannot find enough candidates!!!!!");
			return null;
		}
		return new AIVirtualTargetSelectInfo(burialCount, burialSelectableCards, TargetSelectType.BurialRite, isForbiddenSelectedTarget: true);
	}

	public static List<AIVirtualCard> GetBurialSelectableCards(List<AIVirtualCard> candidates, AIVirtualCard burialOwner, AIVirtualCard originalBurialCard)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsSameCard(burialOwner) && !aIVirtualCard.IsSameCard(originalBurialCard))
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		return list;
	}

	public static AIVirtualCard GetBestBurialRiteTargetForOperationSimulator(AIVirtualCard burialActor, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIVirtualCard> candidates)
	{
		AIScriptTokenArgType timing = AIPreprocessSimulationUtility.ConvertAIOperationTypeToTiming(situation.ActionType);
		if (burialActor.GetBurialRiteCount(field, situation, playPtn, timing) <= 0)
		{
			return null;
		}
		return candidates.FindMax((AIVirtualCard card) => Mathf.Abs(field.AllyPpTotal - card.Cost));
	}

	public static int GetBurialCount(AIScriptTokenArgType burialListType, AIVirtualField field)
	{
		return burialListType switch
		{
			AIScriptTokenArgType.ALLY => field.CardListSet.AllyBurialCards.Count, 
			AIScriptTokenArgType.OPPONENT => field.CardListSet.EnemyBurialCards.Count, 
			_ => 0, 
		};
	}

	public static IEnumerable<BattleCardBase> GetBurialSelectableCards(SkillBase skill, BattleCardBase card)
	{
		if (skill.IsBurialRite)
		{
			return SkillPreprocessBurialRite.GetBurialRiteTarget(card.SelfBattlePlayer, card);
		}
		return null;
	}
}
