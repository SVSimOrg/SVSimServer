using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackDamage : AIWhenAttackSelfAndOtherTagArgument
{
	private AIScriptTokenArgType _selectType;

	private AIPolishConvertedExpression _damage;

	private readonly int DAMAGE_ARG_OFFSET = 1;

	private readonly int SELECT_TYPE_ARG_INDEX = 2;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_ARG_INDEX;

	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIOtherAttackDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damage = _exprList[_exprList.Count - DAMAGE_ARG_OFFSET];
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_ARG_INDEX], base.LegalSelectTypes);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int damage = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
			switch (_selectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
				break;
			}
		}
	}

	private int GetAttackDamageToCertainTarget(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard damageTarget)
	{
		if (damageTarget.IsIndependent)
		{
			return 0;
		}
		if (!CheckTriggerLegal(situation.Actor, tagOwner, playPtn, situation))
		{
			return 0;
		}
		List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
		if (targets.Contains(damageTarget))
		{
			int num = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
			if (_selectType == AIScriptTokenArgType.ALL_SELECT || (_selectType == AIScriptTokenArgType.RANDOM_SELECT && damageTarget.IsSameCard(AIDamageSimulationUtility.SelectDamageTarget(targets, field, playPtn, situation, num, isSpell: false, AISelectTargetPattern.Worst))))
			{
				return num;
			}
		}
		return 0;
	}

	public int PseudoSimulateDamageToTarget(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard damageTarget, AIBarrierPseudoSimulationInfo simBarrier)
	{
		int attackDamageToCertainTarget = GetAttackDamageToCertainTarget(tagOwner, field, playPtn, situation, damageTarget);
		if (attackDamageToCertainTarget > 0)
		{
			bool isSpell = tagOwner.IsSpell;
			int result = simBarrier.SimulateDamageAmount(damageTarget.SimulateDamageShield(attackDamageToCertainTarget, isSkillDamage: true, isSpell), isSpell);
			simBarrier.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
			return result;
		}
		return 0;
	}

	public override void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		AIBarrierPseudoSimulationInfo attackerBarrierInfo = information.AttackerBarrierInfo;
		AIVirtualCard actor = situation.Actor;
		information.AttackerTotalDamage += PseudoSimulateDamageToTarget(tagOwner, field, playPtn, situation, actor, attackerBarrierInfo);
		if (information.AttackerTotalDamage >= actor.Life)
		{
			information.IsAttackerDestroyWhenAttack = true;
		}
	}
}
