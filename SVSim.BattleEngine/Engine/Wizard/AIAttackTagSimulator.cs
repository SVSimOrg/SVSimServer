using System.Collections.Generic;

namespace Wizard;

public static class AIAttackTagSimulator
{
	public static int GetAttackDamageToCertainCard(AIVirtualField field, AIVirtualAttackInfo situation, AIVirtualCard damageTarget)
	{
		if (damageTarget.IsIndependent)
		{
			return 0;
		}
		int num = 0;
		AIVirtualCard actor = situation.Actor;
		List<int> bestPlayPtn = field.BestPlayPtn;
		AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = new AIBarrierPseudoSimulationInfo(damageTarget);
		num += actor.GetAttackDamageToCertainTarget(situation, field, bestPlayPtn, aIBarrierPseudoSimulationInfo);
		if (field.CardListSet.HasOtherAttackDamage())
		{
			for (int i = 0; i < field.CardListSet.OtherAttackDamageHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = field.CardListSet.OtherAttackDamageHolders[i];
				if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.OtherAttackDamage))
				{
					num += aIVirtualCard.TagCollectionContainer.OtherAttackTags.GetAttackDamageToCertainTarget(aIVirtualCard, field, bestPlayPtn, situation, aIBarrierPseudoSimulationInfo);
				}
			}
		}
		return num;
	}

	public static AISimulationBuffInfoCollection GetBuffInfoListWhenCertainAttack(AIVirtualField field, AISituationInfo situation)
	{
		AISimulationBuffInfoCollection aISimulationBuffInfoCollection = new AISimulationBuffInfoCollection();
		AIVirtualCard actor = situation.Actor;
		AIPreprocessSimulationUtility.SimulatePreprocess(actor, situation, field, AIScriptTokenArgType.WHEN_ATTACK, isPseudo: true);
		if (actor.TagCollectionContainer.HasTag(AIPlayTagType.AttackBuff))
		{
			actor.TagCollectionContainer.AttackTags.RegisterBuffInfoToCollection(field, actor, situation, aISimulationBuffInfoCollection);
		}
		if (field.CardListSet.HasOtherAttackBuffHolder)
		{
			for (int i = 0; i < field.CardListSet.OtherAttackBuffTagHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = field.CardListSet.OtherAttackBuffTagHolders[i];
				if (!aIVirtualCard.IsSameCard(actor) && aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.OtherAttackBuff))
				{
					aIVirtualCard.TagCollectionContainer.OtherAttackTags.RegisterBuffInfoToCollection(field, aIVirtualCard, situation, aISimulationBuffInfoCollection);
				}
			}
		}
		AIPreprocessSimulationUtility.ResetPreprocess(situation, field);
		return aISimulationBuffInfoCollection;
	}

	public static void ExecuteAttackByLife(AIVirtualField field, AIVirtualCard attacker, AIVirtualCard target, ref int attackerAtk, ref int targetAtk)
	{
		attacker.AttackByLifeCount = 0;
		target.AttackByLifeCount = 0;
		List<AIVirtualCard> tagHolders = field.CardListSet.GetTagHolders(CardListsForReference.TagHolderReferenceType.AttackByLife);
		if (tagHolders == null || tagHolders.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < tagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = tagHolders[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.AttackByLife))
			{
				AttackByLifeTagCollection attackByLifeTags = aIVirtualCard.TagCollectionContainer.AttackByLifeTags;
				if (attacker.AttackByLifeCount <= 0)
				{
					attackerAtk = attackByLifeTags.GetRealAttack(aIVirtualCard, attacker, attackerAtk);
				}
				if (target.AttackByLifeCount <= 0)
				{
					targetAtk = attackByLifeTags.GetRealAttack(aIVirtualCard, target, targetAtk);
				}
			}
		}
	}

	public static void ApplyOtherAfterAttackOrClashTags(AIVirtualField field, AIVirtualAttackInfo situation)
	{
		if (situation == null || situation.Actor == null || situation.AttackTarget == null)
		{
			return;
		}
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!actor.IsDead && actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAfterAttack))
		{
			actor.TagCollectionContainer.AfterAttackTags.RegisterConditionPassedTags(actor, field, situation);
		}
		if (attackTarget != null && attackTarget.IsUnit)
		{
			if (!actor.IsDead && actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAfterClash))
			{
				actor.TagCollectionContainer.AfterClashTags.RegisterConditionPassedTags(actor, field, situation);
			}
			if (!attackTarget.IsDead && attackTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAfterClash))
			{
				attackTarget.TagCollectionContainer.AfterClashTags.RegisterConditionPassedTags(attackTarget, field, situation);
			}
		}
		for (int i = 0; i < field.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.BothClassAndInplayCards[i];
			if (!aIVirtualCard.IsSameCard(situation.Actor) && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAfterAttack))
			{
				aIVirtualCard.TagCollectionContainer.AfterAttackTags.RegisterConditionPassedTags(aIVirtualCard, field, situation);
			}
		}
	}
}
