using System.Collections.Generic;

namespace Wizard;

public static class AISkillSimulationUtility
{
	public static void GiveSkillToAll(List<AIVirtualCard> targets, AIVirtualField field, AIScriptTokenArgType skillType)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			GiveSkill(targets[i], field, skillType);
		}
	}

	private static void GiveSkillToTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualField field, AIScriptTokenArgType skillType, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("GiveSkillToTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				GiveSkill(aIVirtualCard, field, skillType);
			}
		}
	}

	public static void GiveSkillRandom(List<AIVirtualCard> candidates, int selectCount, AIVirtualField field, AIScriptTokenArgType skillType)
	{
		if (selectCount > 1)
		{
			AIConsoleUtility.LogError("GiveSkillRandom(): 複数選択のキーワード能力付与は未実装です");
			return;
		}
		AIVirtualCard aIVirtualCard = AIBasicTargetSelectUtility.SelectSingleBasicSkillTarget(candidates, skillType, AISelectTargetPattern.Worst);
		if (aIVirtualCard != null)
		{
			GiveSkill(aIVirtualCard, field, skillType);
		}
	}

	public static void ExecuteTargetSelectGiveSkill(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType skillType, AIScriptTokenArgType selectType, int selectCount = 1)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectGiveSkill() Error!! situation is null!!!!!");
		}
		else if (situation.IsTargetExists(selectType))
		{
			GiveSkillToTarget(situation, candidates, field, skillType, selectType);
		}
		else
		{
			GiveSkillToTargetPrediction(candidates, selectCount, field, situation, skillType, selectType);
		}
	}

	private static void GiveSkillToTargetPrediction(List<AIVirtualCard> candidates, int selectCount, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType skillType, AIScriptTokenArgType whichTarget)
	{
		if (selectCount > 1)
		{
			AIConsoleUtility.LogError("GiveSkillToTargetPrediction(): 複数選択のキーワード能力付与は未実装です");
			return;
		}
		AIVirtualCard aIVirtualCard = AIBasicTargetSelectUtility.SelectSingleBasicSkillTarget(candidates, skillType, AISelectTargetPattern.Best);
		if (aIVirtualCard != null)
		{
			situation.SetSingleTargetInInfo(aIVirtualCard, TargetSelectType.Default, whichTarget);
			GiveSkill(aIVirtualCard, field, skillType);
		}
	}

	public static void GiveSkill(AIVirtualCard target, AIVirtualField field, AIScriptTokenArgType skill)
	{
		switch (skill)
		{
		case AIScriptTokenArgType.KILLER:
			target.IsKiller = true;
			break;
		case AIScriptTokenArgType.MEDUSA:
			target.IsDestroyWhenAttack = true;
			break;
		case AIScriptTokenArgType.DRAIN:
			target.IsDrain = true;
			break;
		case AIScriptTokenArgType.GUARD:
			target.IsGuard = true;
			break;
		case AIScriptTokenArgType.QUICK:
			AttachQuick(target, field);
			break;
		case AIScriptTokenArgType.RUSH:
			AttachRush(target, field);
			break;
		case AIScriptTokenArgType.SNEAK:
			target.IsSneak = true;
			break;
		case AIScriptTokenArgType.UNTOUCHABLE:
			target.AddUntouchableCount();
			break;
		case AIScriptTokenArgType.FORCE_TARGETING:
			target.IsForceTargeting = true;
			break;
		case AIScriptTokenArgType.UNBANISHABLE:
			target.IsUnbanishable = true;
			break;
		case AIScriptTokenArgType.IGNORE_GUARD:
			target.IsIgnoreGuard = true;
			break;
		default:
			AIConsoleUtility.LogError("ivnalid skill : tag_ClashSkill");
			break;
		}
	}

	public static void RemoveKeywordSkillToAll(List<AIVirtualCard> targets, AIScriptTokenArgType skillType)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			RemoveKeywordSkill(targets[i], skillType);
		}
	}

	public static void RemoveKeywordSkill(AIVirtualCard target, AIScriptTokenArgType skill)
	{
		switch (skill)
		{
		case AIScriptTokenArgType.GUARD:
			target.IsGuard = false;
			break;
		case AIScriptTokenArgType.UNTOUCHABLE:
			target.SubUntouchableCount();
			break;
		default:
			AIConsoleUtility.LogError($"AISkillSimulationUtility.RemoveKeywordSkill(): Invalid skill type. type:{skill}");
			break;
		}
	}

	public static bool HasSkill(AIVirtualCard target, AIScriptTokenArgType skill, EnemyAI ai, List<int> playPtn, bool isNot = false)
	{
		if (!target.IsInHand)
		{
			return target.IsHoldKeywordSkill(skill);
		}
		int num = ai.CurrentVirtualField.AllyHandCards.FindIndex((AIVirtualCard c) => c.IsSameCard(target));
		if (num < 0)
		{
			return false;
		}
		if (target.TagCollectionContainer.HasTag(AIPlayTagType.GiveSkill) && target.TagCollectionContainer.GiveSkillTags.ExecuteSkill(target, playPtn, skill))
		{
			return true;
		}
		AIHandPlayEstimator handPlayEstimator = ai.CurrentVirtualField.AllyHandCards[num].GetHandPlayEstimator();
		if (handPlayEstimator == null || handPlayEstimator.InplayEstimations == null || handPlayEstimator.InplayEstimations.Count <= 0)
		{
			return false;
		}
		for (int num2 = 0; num2 < handPlayEstimator.InplayEstimations.Count; num2++)
		{
			if (HasSkill(handPlayEstimator.InplayEstimations[num2], skill, ai, EnemyAI.EmptyPlayPtn, isNot))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsFollowerOnlySkillType(AIScriptTokenArgType skillType)
	{
		if (skillType != AIScriptTokenArgType.QUICK && skillType != AIScriptTokenArgType.RUSH && skillType != AIScriptTokenArgType.GUARD && skillType != AIScriptTokenArgType.DRAIN && skillType != AIScriptTokenArgType.KILLER && skillType != AIScriptTokenArgType.SNEAK)
		{
			return skillType == AIScriptTokenArgType.IGNORE_GUARD;
		}
		return true;
	}

	public static void HealAll(List<AIVirtualCard> targets, AIVirtualField field, int heal, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (aIVirtualCard.IsOnField && !aIVirtualCard.IsAmulet && !aIVirtualCard.IsCountdownAmulet && !aIVirtualCard.IsIndependent)
			{
				int healLife = AIHealSimulationUtility.CalcHealModifier(aIVirtualCard, playPtn, situation, heal);
				aIVirtualCard.Heal(healLife);
				list.Add(aIVirtualCard);
			}
		}
		if (list.Count > 0)
		{
			HealActivation(list, field, playPtn, situation);
		}
	}

	public static void HealSingle(AIVirtualCard target, AIVirtualField field, int heal, List<int> playPtn, AISituationInfo situation)
	{
		if (target.IsOnField && !target.IsAmulet && !target.IsCountdownAmulet && !target.IsIndependent)
		{
			int healLife = AIHealSimulationUtility.CalcHealModifier(target, playPtn, situation, heal);
			target.Heal(healLife);
			HealActivation(new List<AIVirtualCard> { target }, field, playPtn, situation);
		}
	}

	public static void HealTarget(AISituationInfo situation, AIVirtualField field, AIScriptTokenArgType whichTarget, int heal)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			List<AIVirtualCard> targets = situationTarget.Targets;
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			HealAll(targets, field, heal, emptyPlayPtn, situation);
		}
	}

	public static void HealTargetPrediction(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard healOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType whichTarget, int heal)
	{
		AIVirtualCard target = AIHealSimulationUtility.SelectBestTarget(AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, healOwner, playPtn), heal);
		situation.SetSingleTargetInInfo(target, TargetSelectType.Default, whichTarget);
		HealTarget(situation, field, whichTarget, heal);
	}

	private static void HealActivation(List<AIVirtualCard> healedTargets, AIVirtualField field, List<int> playptn, AISituationInfo situation)
	{
		List<AIVirtualCard> tagHolders = field.CardListSet.GetTagHolders(CardListsForReference.TagHolderReferenceType.WhenHeal);
		if (tagHolders == null || tagHolders.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < healedTargets.Count; i++)
		{
			AIVirtualCard healedCard = healedTargets[i];
			for (int j = 0; j < tagHolders.Count; j++)
			{
				AIVirtualCard aIVirtualCard = tagHolders[j];
				if (!aIVirtualCard.IsDead && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenHeal))
				{
					aIVirtualCard.TagCollectionContainer.HealTags.RegisterPassedConditionTags(aIVirtualCard, healedCard, playptn, situation);
				}
			}
		}
		field.AllActivateCountHolderIncrement(situation, AIPlayTagType.HealActivateCount, healedTargets);
	}

	public static void DestroyAll(List<AIVirtualCard> targets, AIVirtualField field, AISituationInfo situation)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (!targets[i].IsDead && !targets[i].IsIndependent)
			{
				targets[i].RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
			}
		}
	}

	public static void DestroyOldest(List<AIVirtualCard> targets, AIVirtualField field, AISituationInfo situation, int selectCount = 1)
	{
		List<AIVirtualCard> targets2 = ((targets.Count > selectCount) ? targets.GetRange(0, selectCount) : targets);
		DestroyAll(targets2, field, situation);
	}

	public static void DestroyRandom(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int selectCount = 1)
	{
		if (selectCount <= 1)
		{
			AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectRemovalTarget(targets, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Destroy);
			if (aIVirtualCard != null && !aIVirtualCard.IsDead && !aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
			}
		}
		else
		{
			DestroyAll(AISimulationRemovalUtility.SelectMultipleRemovalTargets(targets, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Destroy, selectCount), field, situation);
		}
	}

	public static void DestroyTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("DestroyTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard) && !aIVirtualCard.IsIndependent)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
			}
		}
	}

	public static void ExecuteTargetSelectDiscard(List<AIVirtualCard> candidates, int discardCount, AIScriptTokenArgType selectType, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsAlly)
		{
			if (situation == null)
			{
				AIConsoleUtility.LogError("ExecuteTargetSelectDiscard() Error!! situation is null!!!!!");
			}
			else if (situation.IsTargetExists(selectType))
			{
				DiscardTarget(tagOwner, selectType, field, situation);
			}
			else
			{
				DiscardTargetPrediction(candidates, discardCount, tagOwner, field, playPtn, situation);
			}
		}
	}

	private static void DiscardTarget(AIVirtualCard ownerCard, AIScriptTokenArgType selectType, AIVirtualField field, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(selectType);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			ExecuteDiscard(ownerCard, situationTarget.Targets, field, situation);
		}
	}

	private static void DiscardTargetPrediction(List<AIVirtualCard> candidates, int discardCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIDiscardUtility.SelectBestDiscardTarget(tagOwner, field, candidates, discardCount, tagOwner.SelfField.BestPlayPtn, situation);
		if (list != null && list.Count > 0)
		{
			ExecuteDiscard(tagOwner, list, field, situation);
		}
	}

	public static void DiscardRandom(AIVirtualCard ownerCard, AIVirtualField field, List<AIVirtualCard> discardCandidates, int discardCount, AISituationInfo situation)
	{
		if (ownerCard.IsAlly)
		{
			List<AIVirtualCard> discardTargets = AIDiscardUtility.SelectWorstDiscardTarget(ownerCard, field, discardCandidates, discardCount, field.BestPlayPtn, situation);
			ExecuteDiscard(ownerCard, discardTargets, field, situation);
		}
	}

	public static void DiscardAll(AIVirtualCard ownerCard, List<AIVirtualCard> discardCandidates, AIVirtualField field, AISituationInfo situation)
	{
		if (ownerCard.IsAlly)
		{
			ExecuteDiscard(ownerCard, discardCandidates, field, situation);
		}
	}

	public static void ExecuteDiscard(AIVirtualCard ownerCard, List<AIVirtualCard> discardTargets, AIVirtualField field, AISituationInfo situation)
	{
		if (discardTargets == null || discardTargets.Count <= 0 || field.AllyHandCards.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < discardTargets.Count; i++)
		{
			AIVirtualCard hand = discardTargets[i];
			field.RemoveAllyHandCard(hand);
		}
		AIDiscardInfo discardInfo = new AIDiscardInfo(ownerCard, isSuccess: true, discardTargets);
		situation.SetDiscardInfo(discardInfo);
		for (int j = 0; j < discardTargets.Count; j++)
		{
			AIVirtualCard aIVirtualCard = discardTargets[j];
			field.SimulationExtraBonus += AIDiscardUtility.EvaluateDiscardedBonus(aIVirtualCard, field.BestPlayPtn, situation, field, isIgnoreInBattle: true, isCalcCostDiff: false, isCalcTokenValue: false);
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenDiscarded))
			{
				aIVirtualCard.TagCollectionContainer.DiscardedTags.RegisterPassedConditionTags(aIVirtualCard, field.BestPlayPtn, situation);
			}
		}
		if (field.CardListSet.HasAfterDiscardTagHolder)
		{
			for (int k = 0; k < field.CardListSet.AfterDiscardTagHolders.Count; k++)
			{
				field.CardListSet.AfterDiscardTagHolders[k].TagCollectionContainer.AfterDiscardTags.RegisterPassedConditionTags(ownerCard, field.BestPlayPtn, situation);
			}
		}
	}

	private static void AttachRush(AIVirtualCard card, AIVirtualField field)
	{
		if (!card.IsRush && !card.IsQuick)
		{
			card.IsRush = true;
			card.IsSummonDrunkenness = false;
		}
	}

	private static void AttachQuick(AIVirtualCard card, AIVirtualField field)
	{
		if (!card.IsQuick)
		{
			card.IsQuick = true;
			card.IsSummonDrunkenness = false;
		}
	}

	public static void SetStatusAll(List<AIVirtualCard> targets, int attack, int life, AISituationInfo situation)
	{
		if (attack < 0 || life < 0)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (aIVirtualCard.IsUnit)
			{
				aIVirtualCard.SetNewStatus(situation, attack, life);
			}
		}
	}

	public static void SetStatusTarget(List<AIVirtualCard> candidates, AIScriptTokenArgType whichTarget, int attack, int life, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("SetStatusTarget error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				SetStatusSingle(aIVirtualCard, attack, life, situation);
			}
		}
	}

	public static void SetStatusSingle(AIVirtualCard target, int attack, int life, AISituationInfo situation)
	{
		if (attack >= 0 && life >= 0 && target.IsUnit)
		{
			target.SetNewStatus(situation, attack, life);
		}
	}

	public static void ModifierNotConsumeEpALL(List<AIVirtualCard> targets)
	{
		if (targets == null || targets.Count <= 0)
		{
			AIConsoleUtility.LogError("ModifierNotConsumeEpALL() : Target List is Missing Error !!");
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			ModifierNotConsumeEpSingle(targets[i]);
		}
	}

	public static void ModifierNotConsumeEpTargets(List<AIVirtualCard> candidate, AIScriptTokenArgType selectType, AISituationInfo situation)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(selectType);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("ModifierNotConsumeEpTargets() : Selected Target is Missing Error !!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (candidate.Contains(aIVirtualCard))
			{
				ModifierNotConsumeEpSingle(aIVirtualCard);
			}
		}
	}

	public static void ModifierNotConsumeEpSingle(AIVirtualCard card)
	{
		if (card == null)
		{
			AIConsoleUtility.LogError("ModifierNotConsumeEpSingle() : Target card is Missing Error !!");
		}
		else if (card.IsUnit && !card.IsDead)
		{
			card.IsNotConsumeEp = true;
		}
	}
}
