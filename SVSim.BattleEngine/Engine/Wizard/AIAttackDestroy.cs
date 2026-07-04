using System.Collections.Generic;

namespace Wizard;

public class AIAttackDestroy : AIWhenAttackOrWhenFightTagArgument
{
	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIAttackDestroy(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DestroyAll(targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			default:
				LogSelectTypeError(base.SelectType);
				break;
			}
		}
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		if (target.IsIndependent || target.IsIndestructible)
		{
			return false;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Contains(target))
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				return true;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				AIVirtualCard card = AISimulationRemovalUtility.SelectRemovalTarget(targetsFromField, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Destroy);
				return target.IsSameCard(card);
			}
			}
		}
		return false;
	}

	public override bool CanKillAnyTarget(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, List<AIBarrierPseudoSimulationInfo> simBarrierList, int[] realDamageList)
	{
		if (targetList == null || targetList.Count <= 0)
		{
			return false;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return false;
		}
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int i = 0; i < targetList.Count; i++)
			{
				if (targetList[i].IsSameCardIncluded(targetsFromField))
				{
					return true;
				}
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
			return AISimulationRemovalUtility.SelectRemovalTarget(targetsFromField, tagOwner, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Destroy).IsSameCardIncluded(targetList);
		}
		return false;
	}

	private void LogSelectTypeError(AIScriptTokenArgType selectType)
	{
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
