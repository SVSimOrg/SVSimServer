using System.Collections.Generic;

namespace Wizard;

public class AIEvoDamage : AIEvoTagArgument
{
	private AIPolishConvertedExpression _damage;

	private AIPolishConvertedExpression _count;

	private readonly int COUNT_ARG_OFFSET = 1;

	private readonly int DAMAGE_ARG_OFFSET = 2;

	private int _countTemp;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIEvoDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damage = _exprList[_exprList.Count - DAMAGE_ARG_OFFSET];
		_count = _exprList[_exprList.Count - COUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int num = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
		int num2 = (int)_count.EvalArg(tagOwner, playPtn, field, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int j = 0; j < num2; j++)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, num, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int i = 0; i < num2; i++)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, num, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			AIDamageSimulationUtility.DamageRandomMultiSelect(targetsFromField, tagOwner, field, num, num2, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			AIDamageSimulationUtility.ExecuteTargetSelectDamage(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType, num, num2);
			break;
		case AIScriptTokenArgType.DIVIDED_SELECT:
			AIDamageSimulationUtility.DamageOldOrderedTargets(targetsFromField, num, tagOwner, field, playPtn, situation);
			break;
		case AIScriptTokenArgType.REVERSE_TARGET:
		case AIScriptTokenArgType.FIRST_SELECT:
			break;
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent)
		{
			return false;
		}
		AIVirtualField selfField = owner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		_countTemp = (int)_count.EvalArg(owner, bestPlayPtn, selfField, situation);
		if (!IsCertainlyIncludeTarget(owner, candidate, situation))
		{
			return false;
		}
		int damageAmount = (int)_damage.EvalArg(owner, bestPlayPtn, selfField, situation);
		return candidate.SimulateDamageAmount(damageAmount, isSkillDamage: true, owner.IsSpell) * _countTemp >= candidate.Life;
	}

	protected override bool CheckIsCandidateSelectedBySelectType(List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualCard candidate)
	{
		if (base.SelectType == AIScriptTokenArgType.RANDOM_MULTI_SELECT)
		{
			return targets.Count <= _countTemp;
		}
		return base.CheckIsCandidateSelectedBySelectType(targets, situation, candidate);
	}

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead: true);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[6]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT,
			AIScriptTokenArgType.DIVIDED_SELECT
		};
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Damage;
	}
}
