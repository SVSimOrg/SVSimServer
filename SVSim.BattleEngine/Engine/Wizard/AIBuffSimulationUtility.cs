using System.Collections.Generic;

namespace Wizard;

public static class AIBuffSimulationUtility
{
	public static void BuffAll_old(List<AIVirtualCard> targets, AIVirtualField field, AIBuffExecutingInfo_old buffInfo, bool isTemp, List<int> playPtn, AISituationInfo situation)
	{
		if (buffInfo.IsEmpty())
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (aIVirtualCard.IsUnit)
			{
				aIVirtualCard.GiveBuff(situation, buffInfo, isTemp);
			}
		}
		if (buffInfo.IsBuff())
		{
			BuffActivation(targets, field, playPtn, situation);
		}
	}

	public static void BuffRandom_old(List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIBuffExecutingInfo_old buffInfo, bool isTemp)
	{
		AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectWorstTargetForBuff(candidates, buffInfo);
		if (aIVirtualCard != null)
		{
			BuffSingle_old(aIVirtualCard, field, buffInfo, isTemp, playPtn, situation);
		}
	}

	public static void BuffTarget_old(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType whichTarget, AIBuffExecutingInfo_old buffInfo, bool isTemp)
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
			if (candidates.Contains(aIVirtualCard))
			{
				BuffSingle_old(aIVirtualCard, field, buffInfo, isTemp, playPtn, situation);
			}
		}
	}

	public static void BuffSingle_old(AIVirtualCard target, AIVirtualField field, AIBuffExecutingInfo_old buffInfo, bool isTemp, List<int> playPtn, AISituationInfo situation)
	{
		if (!buffInfo.IsEmpty() && target.IsUnit)
		{
			target.GiveBuff(situation, buffInfo, isTemp);
			if (buffInfo.IsBuff())
			{
				BuffActivation(new List<AIVirtualCard> { target }, field, playPtn, situation);
			}
		}
	}

	public static void BuffFirst_old(List<AIVirtualCard> targets, AIVirtualField field, AIBuffExecutingInfo_old buffInfo, bool isTemp, List<int> playPtn, AISituationInfo situation)
	{
		if (targets != null && targets.Count > 0)
		{
			BuffSingle_old(targets[0], field, buffInfo, isTemp, playPtn, situation);
		}
	}

	private static void BuffActivation(List<AIVirtualCard> buffTargets, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (buffTargets == null || buffTargets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < buffTargets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = buffTargets[i];
			if (!aIVirtualCard.IsDead)
			{
				if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.BuffBonus))
				{
					field.ApplyBuffBonus(aIVirtualCard, playPtn, situation);
				}
				if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenBuff))
				{
					aIVirtualCard.TagCollectionContainer.BuffTriggerTags.Execute(aIVirtualCard, aIVirtualCard, playPtn, situation);
				}
			}
		}
		field.AllActivateCountHolderIncrement(situation, AIPlayTagType.BuffActivateCounnt, buffTargets);
	}

	public static AIBuffExecutingInfo_old GetBuffExecutingInfo_old(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, AIPolishConvertedExpression attackExpression, AIPolishConvertedExpression lifeExpression)
	{
		int attackValue = (int)attackExpression.EvalArg(tagOwner, playPtn, field, situation);
		int lifeValue = (int)lifeExpression.EvalArg(tagOwner, playPtn, field, situation);
		return new AIBuffExecutingInfo_old
		{
			AttackValue = attackValue,
			LifeValue = lifeValue,
			IsMultiplyAttack = attackExpression.IsMultiplyMarked,
			IsMultiplyLife = lifeExpression.IsMultiplyMarked
		};
	}
}
