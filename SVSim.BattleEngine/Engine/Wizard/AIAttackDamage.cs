using System.Collections.Generic;

namespace Wizard;

public class AIAttackDamage : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _damage;

	private readonly int DAMAGE_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIAttackDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damage = _exprList[_exprList.Count - DAMAGE_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
				break;
			}
		}
	}

	private int GetAttackDamageToCertainTarget(AIVirtualCard tagOwner, AISituationInfo situation, AIVirtualField field, List<int> playPtn, AIVirtualCard damageTarget)
	{
		if (damageTarget.IsIndependent)
		{
			return 0;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField.Contains(damageTarget))
		{
			int num = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT || (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT && damageTarget.IsSameCard(AIDamageSimulationUtility.SelectDamageTarget(targetsFromField, field, playPtn, situation, num, isSpell: false, AISelectTargetPattern.Worst))))
			{
				return num;
			}
		}
		return 0;
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		totalDamage += PseudoSimulateWhenAttackDamageToCertainCard(tagOwner, target, field, situation, playPtn, simBarrier);
		return totalDamage >= target.Life;
	}

	public override int PseudoSimulateWhenAttackDamageToCertainCard(AIVirtualCard tagOwner, AIVirtualCard damageTarget, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		int attackDamageToCertainTarget = GetAttackDamageToCertainTarget(tagOwner, situation, field, playPtn, damageTarget);
		if (attackDamageToCertainTarget <= 0)
		{
			return 0;
		}
		bool isSpell = tagOwner.IsSpell;
		int result = simBarrier.SimulateDamageAmount(damageTarget.SimulateDamageShield(attackDamageToCertainTarget, isSkillDamage: true, isSpell), isSpell);
		simBarrier.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
		return result;
	}

	public override bool CanKillAnyTarget(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, List<AIBarrierPseudoSimulationInfo> simBarrierList, int[] realDamageList)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return false;
		}
		int num = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
		if (num <= 0)
		{
			return false;
		}
		bool isSpell = tagOwner.IsSpell;
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			if (aIVirtualCard.IsIndependent || !aIVirtualCard.IsSameCardIncluded(targetsFromField))
			{
				continue;
			}
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = simBarrierList[i];
			int num2 = aIBarrierPseudoSimulationInfo.SimulateDamageAmount(aIVirtualCard.SimulateDamageShield(num, isSkillDamage: true, isSpell), isSpell);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT || (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT && aIVirtualCard.IsSameCard(AIDamageSimulationUtility.SelectDamageTarget(targetsFromField, field, playPtn, situation, num2, isSpell: false, AISelectTargetPattern.Worst))))
			{
				aIBarrierPseudoSimulationInfo.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
				realDamageList[i] += num2;
				if (realDamageList[i] >= aIVirtualCard.Life)
				{
					return true;
				}
			}
		}
		return false;
	}
}
