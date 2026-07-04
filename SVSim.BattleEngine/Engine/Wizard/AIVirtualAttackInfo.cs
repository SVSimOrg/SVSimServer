using System.Collections.Generic;

namespace Wizard;

public class AIVirtualAttackInfo : AIVirtualActionInfo
{
	public bool IsBreakTarget;

	public bool IsAttackSuccessed;

	public AIVirtualCard AttackTarget { get; private set; }

	public bool IsAttackFollower { get; private set; }

	public AIAttackPreCheckInformation PreCheckInformation { get; private set; }

	public bool IsUsePreCheck => PreCheckInformation != null;

	public AIVirtualAttackInfo(AIVirtualCard sourceCard, bool isAttackFollower, AIVirtualActionInfo premise = null)
		: base(sourceCard, AIOperationType.ATTACK, null)
	{
		IsAttackFollower = isAttackFollower;
		IsBreakTarget = false;
		IsAttackSuccessed = false;
		PreCheckInformation = null;
	}

	public AIVirtualAttackInfo(AIVirtualCard sourceCard, AIVirtualCard target)
		: base(sourceCard, AIOperationType.ATTACK, null)
	{
		SetAttackTarget(target);
		IsAttackFollower = true;
		IsBreakTarget = false;
		IsAttackSuccessed = false;
		PreCheckInformation = null;
	}

	public void SetAttackTarget(AIVirtualCard target)
	{
		AttackTarget = target;
	}

	public override ulong GetHash()
	{
		return base.GetHash() * 1262293 + ((AttackTarget != null) ? AttackTarget.GetHash() : 0);
	}

	public void SetPreCheckInformation(AIAttackPreCheckInformation info)
	{
		PreCheckInformation = info;
	}

	public void PseudoSimulateForEvalInstantAttack(AIVirtualField field, List<int> playPtn, EvalInstantAttackInformation information)
	{
		AIPreprocessSimulationUtility.SimulatePreprocess(base.Actor, this, field, AIScriptTokenArgType.WHEN_ATTACK, isPseudo: true);
		if (base.Actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			base.Actor.TagCollectionContainer.AttackTags.PseudoExecuteForEvalInstantAttack(base.Actor, this, field, playPtn, information);
		}
		if (AttackTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			AttackTagCollection attackTags = AttackTarget.TagCollectionContainer.AttackTags;
			if (attackTags.HasClashTag)
			{
				attackTags.PseudoExecuteForEvalInstantAttack(AttackTarget, this, field, playPtn, information);
			}
		}
		for (int i = 0; i < field.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.BothClassAndInplayCards[i];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherAttack))
			{
				aIVirtualCard.TagCollectionContainer.OtherAttackTags.PseudoExecuteForEvalInstantAttack(aIVirtualCard, this, field, playPtn, information);
			}
		}
		AIPreprocessSimulationUtility.ResetPreprocess(this, field);
	}

	public bool WillTargetDestroyByAttackTags(AIVirtualField field, List<int> playPtn, AIVirtualCard destroyTarget)
	{
		AIBarrierPseudoSimulationInfo simBarrier = new AIBarrierPseudoSimulationInfo(destroyTarget);
		if (base.Actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack) && base.Actor.TagCollectionContainer.AttackTags.WillSingleTargetDestroyByAttackTags(base.Actor, this, field, playPtn, simBarrier))
		{
			return true;
		}
		if (AttackTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			AttackTagCollection attackTags = AttackTarget.TagCollectionContainer.AttackTags;
			if (attackTags.HasClashTag && attackTags.WillSingleTargetDestroyByAttackTags(AttackTarget, this, field, playPtn, simBarrier))
			{
				return true;
			}
		}
		for (int i = 0; i < field.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.BothClassAndInplayCards[i];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherAttack) && aIVirtualCard.TagCollectionContainer.OtherAttackTags.CheckDestroyByAttackTags(aIVirtualCard, this, field, playPtn, simBarrier))
			{
				return true;
			}
		}
		return false;
	}

	public bool WillAnyOtherAttackerDestroyed(AIVirtualField field, List<int> playPtn)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (aIVirtualCard.IsAttackable(EnemyAI.EmptyPlayPtn))
			{
				list.Add(aIVirtualCard);
			}
		}
		if (base.Actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack) && base.Actor.TagCollectionContainer.AttackTags.WillAnyTargetDestroyByAttackTags(base.Actor, this, field, playPtn, list))
		{
			return true;
		}
		if (AttackTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			AttackTagCollection attackTags = AttackTarget.TagCollectionContainer.AttackTags;
			if (attackTags.HasClashTag && attackTags.WillAnyTargetDestroyByAttackTags(AttackTarget, this, field, playPtn, list))
			{
				return true;
			}
		}
		return false;
	}
}
