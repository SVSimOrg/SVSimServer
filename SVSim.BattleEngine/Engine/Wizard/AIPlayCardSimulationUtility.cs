using System.Collections.Generic;

namespace Wizard;

public static class AIPlayCardSimulationUtility
{
	public static void ExecuteRestPlayPtnAfterAttack(AIVirtualField field, ref List<AIVirtualActionInfo> moves)
	{
		if (field.BestPlayPtn == null || field.BestPlayPtn.Count <= 0)
		{
			return;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		int num = field.BestPlayPtn.Count;
		while (bestPlayPtn.Count > 0)
		{
			num--;
			if (num < 0)
			{
				AIConsoleUtility.LogError("ExecuteRestPlayPtnAfterAttack() error!! expectedPlayPtnCount turns to -1!!!!!");
				break;
			}
			AIVirtualCard aIVirtualCard = field.AllyHandCards[bestPlayPtn[0]];
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
			if (field.IsCannotPlayByTag(aIVirtualTargetSelectAction, bestPlayPtn))
			{
				break;
			}
			PlaySimulationType playType;
			int useCost = aIVirtualCard.GetUseCost(field.AllyPp, EnemyAI.EmptyPlayPtn, aIVirtualTargetSelectAction, out playType);
			if (useCost >= 0)
			{
				if (playType == PlaySimulationType.ChoiceTransform)
				{
					AISelectedTargetInfo choiceTargets = aIVirtualCard.GetChoiceSelectInfo(field, aIVirtualTargetSelectAction).GetChoiceTargets(aIVirtualCard, field, bestPlayPtn, aIVirtualTargetSelectAction);
					AIVirtualCard aIVirtualCard2 = AITokenManager.ProcessToken(choiceTargets.FirstTarget.BaseCard, field);
					aIVirtualCard2.BeforeTransformedCardForSimulation = aIVirtualCard;
					aIVirtualTargetSelectAction.SetActor(aIVirtualCard2);
					aIVirtualTargetSelectAction.SetChoicedMultipleTargetInInfo(choiceTargets.Targets);
				}
				else
				{
					SetRealActorToPlaySituation(aIVirtualTargetSelectAction, field, useCost, playType);
				}
				AIVirtualCard actor = aIVirtualTargetSelectAction.Actor;
				int num2 = ((actor.IsUnit || actor.IsAmulet) ? 1 : 0);
				if (field.AllyInplayCards.Count + num2 > 5 || !CheckTargetSelectPlayCondition(aIVirtualTargetSelectAction, field, playType))
				{
					break;
				}
				PlaySimulationInfo playSimulationInfo = CreatePlaySimulationInfo(actor, aIVirtualTargetSelectAction, field);
				if (playSimulationInfo == null)
				{
					break;
				}
				AIVirtualActionInfo item = new AIVirtualTargetSelectAction(actor, aIVirtualCard, AIOperationType.PLAY);
				moves.Insert(moves.Count - 1, item);
				AIVirtualPlaySimulator.PlayCard(aIVirtualTargetSelectAction, field, playSimulationInfo);
				continue;
			}
			break;
		}
	}

	public static PlaySimulationInfo CreatePlaySimulationInfo(AIVirtualCard actor, AISituationInfo situation, AIVirtualField field)
	{
		AIVirtualCard aIVirtualCard = ((situation != null) ? situation.OriginalCard : actor);
		if (aIVirtualCard.IsAlly && !aIVirtualCard.BasePlayable())
		{
			return null;
		}
		int restPp = (aIVirtualCard.IsAlly ? field.AllyPp : field.EnemyPp);
		PlaySimulationType playType;
		int useCost = aIVirtualCard.GetUseCost(restPp, null, situation, out playType);
		if (useCost < 0)
		{
			return null;
		}
		if (actor.IsAlly && !actor.IsSpell && playType != PlaySimulationType.Accelerate)
		{
			int num = 0;
			for (int i = 0; i < field.AllyInplayCards.Count; i++)
			{
				if (!field.AllyInplayCards[i].IsDead)
				{
					num++;
				}
			}
			if (num >= 5)
			{
				return null;
			}
		}
		return new PlaySimulationInfo(actor, useCost, playType);
	}

	public static void UpdateFieldWhenEvaluateSpellCard(AIVirtualCard playCard, AIVirtualField field)
	{
		AISpellboostSimulationUtility.SpellboostWhenPlaySpellAtEvaluation(playCard, field);
		field.VirtualCemetery.AddCemetery(1, playCard.IsAlly);
	}

	public static bool IsAbleToPlayCard(AIVirtualTargetSelectAction play, AIVirtualField field, List<int> playPtn = null)
	{
		if (play.ActionType != AIOperationType.PLAY)
		{
			return false;
		}
		AIVirtualCard actor = play.Actor;
		AIVirtualCard originalCard = play.OriginalCard;
		if (!actor.IsAlly || !actor.IsSelfTurn)
		{
			return false;
		}
		if (!field.AllyHandCards.Contains(originalCard))
		{
			return false;
		}
		if (field.IsCannotPlayByTag(play, playPtn))
		{
			return false;
		}
		if (field.AI.PlayPtnRecorder.GetFixedCostWithCheckingPlayType(originalCard, field, playPtn, out var playType) < 0)
		{
			return false;
		}
		if (!CheckTargetSelectPlayCondition(play, field, playType))
		{
			return false;
		}
		return true;
	}

	public static bool CheckTargetSelectPlayCondition(AIVirtualTargetSelectAction play, AIVirtualField field, PlaySimulationType playType)
	{
		AIVirtualCard actor = play.Actor;
		if (!actor.IsSpell && playType != PlaySimulationType.Accelerate)
		{
			return true;
		}
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			PlayTagCollection playTags = actor.TagCollectionContainer.PlayTags;
			List<AIPlayTag> conditionPassedTargetSelectTagList = playTags.GetConditionPassedTargetSelectTagList(actor, field, play, emptyPlayPtn);
			if (conditionPassedTargetSelectTagList != null && conditionPassedTargetSelectTagList.Count > 0)
			{
				if (!CheckTargetSelectCandidateCount(actor, play, field, conditionPassedTargetSelectTagList))
				{
					return false;
				}
				List<AIPlayTag> conditionPassedTargetSelectTagList2 = playTags.GetConditionPassedTargetSelectTagList(actor, field, play, emptyPlayPtn, AIScriptTokenArgType.SECOND_TARGET_SELECT);
				if (conditionPassedTargetSelectTagList2 != null && conditionPassedTargetSelectTagList2.Count > 0 && !CheckTargetSelectCandidateCount(actor, play, field, conditionPassedTargetSelectTagList2))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool CheckTargetSelectCandidateCount(AIVirtualCard playCard, AIVirtualTargetSelectAction play, AIVirtualField field, List<AIPlayTag> targetSelectTagList)
	{
		for (int i = 0; i < targetSelectTagList.Count; i++)
		{
			AIWhenPlayTagArgument obj = targetSelectTagList[i].ArgumentExpressions as AIWhenPlayTagArgument;
			List<AIVirtualCard> targetSelectCandidates = obj.GetTargetSelectCandidates(playCard, field, play);
			int selectCount = obj.GetSelectCount(playCard, field, EnemyAI.EmptyPlayPtn, play);
			if (targetSelectCandidates != null && targetSelectCandidates.Count >= selectCount)
			{
				return true;
			}
		}
		return false;
	}

	public static void CreateWhenPlayTagExecutingQueue(AIVirtualTargetSelectAction situation, AIVirtualField field, PlaySimulationType playType)
	{
		AIVirtualCard actor = situation.Actor;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		actor.EnqueueGiveSkill(field, emptyPlayPtn, situation);
		field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			actor.TagCollectionContainer.FanfareTags.EnqueueConditionPassedTags(situation, field, emptyPlayPtn, isRemovalCheck: false);
		}
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			actor.TagCollectionContainer.PlayTags.EnqueueConditionPassedTags(situation, field, emptyPlayPtn, isRemovalCheck: false);
		}
		if (situation.IsChoiceBrave || field.CardListSet.OtherPlayTagHolders == null)
		{
			return;
		}
		List<AIVirtualCard> otherPlayTagHolders = field.CardListSet.OtherPlayTagHolders;
		for (int i = 0; i < otherPlayTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = otherPlayTagHolders[i];
			if (!aIVirtualCard.IsSameCard(actor))
			{
				aIVirtualCard.TagCollectionContainer.OtherPlayTags.EnqueueConditionPassedTags(aIVirtualCard, situation, field, emptyPlayPtn, playType);
			}
		}
	}

	public static void SetChoiceTargetAsActor(AIVirtualTargetSelectAction situation)
	{
		AISelectedTargetInfo choiceInfo = situation.SelectedTargets.GetChoiceInfo();
		if (choiceInfo == null)
		{
			AIConsoleUtility.LogError("SetChoiceTargetAsActor error!! cannot find choiceTargetInfo!!!!!");
			return;
		}
		AIVirtualCard firstTarget = choiceInfo.FirstTarget;
		AIVirtualField selfField = situation.Actor.SelfField;
		AIVirtualCard aIVirtualCard = AITokenManager.ProcessToken(firstTarget.BaseCard, selfField);
		selfField.AI.tokenManager.AddTokenFromCard(aIVirtualCard);
		situation.SetActor(aIVirtualCard);
	}

	private static int GetTransformCardId(AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AIVirtualTargetSelectAction situation, int usedCost, PlaySimulationType playType)
	{
		int result = -1;
		switch (playType)
		{
		case PlaySimulationType.Accelerate:
			result = AIAccelerateUtility.GetConditionPassedAccelerateId(playCard, field, playPtn, situation, usedCost);
			break;
		case PlaySimulationType.Crystalize:
			result = AICrystalizeUtility.GetCrystalizeId(playCard, usedCost);
			break;
		}
		return result;
	}

	public static bool SetRealActorToPlaySituation(AIVirtualTargetSelectAction play, AIVirtualField field, int usedCost, PlaySimulationType playType)
	{
		List<int> bestPlayPtn = field.BestPlayPtn;
		AIVirtualCard originalCard = play.OriginalCard;
		int transformCardId = GetTransformCardId(originalCard, field, bestPlayPtn, play, usedCost, playType);
		if (transformCardId <= 0)
		{
			return false;
		}
		AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(transformCardId, originalCard.IsAlly, field, needsClone: true);
		tokenFromId.BeforeTransformedCardForSimulation = originalCard;
		play.SetActor(tokenFromId);
		field.AI.tokenManager.AddTokenFromCard(tokenFromId);
		return true;
	}
}
