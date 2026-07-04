using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackBuff : AIWhenAttackSelfAndOtherTagArgument
{
	private AIScriptTokenArgType _selectType;

	private AIPolishConvertedExpression _attack;

	private AIPolishConvertedExpression _life;

	private AIScriptTokenArgType _tempOrPerm;

	private readonly int TEMP_OR_PERM_ARG_OFFSET = 1;

	private readonly int LIFE_ARG_OFFSET = 2;

	private readonly int ATTACK_ARG_OFFSET = 3;

	private readonly int SELECT_TYPE_OFFSET = 4;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIOtherAttackBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		_tempOrPerm = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - TEMP_OR_PERM_ARG_OFFSET], AIBuffEvaluationUtility.LEGAL_TEMP_OR_PERM_ARGUMENTS);
		_attack = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		_life = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !_attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attack, _life);
			bool isTemp = _tempOrPerm == AIScriptTokenArgType.TEMP;
			if (_selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp, playPtn, situation);
			}
		}
	}

	public void RegisterBuffInfo(AISimulationBuffInfoCollection collection, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, ulong tagHash)
	{
		AIVirtualCard actor = situation.Actor;
		if (!CheckTriggerLegal(actor, tagOwner, playPtn, situation))
		{
			return;
		}
		List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attack, _life);
			if (!buffExecutingInfo_old.IsMultiplyAttack && !buffExecutingInfo_old.IsMultiplyLife)
			{
				bool num = _tempOrPerm == AIScriptTokenArgType.TEMP;
				int attackValue = buffExecutingInfo_old.AttackValue;
				int lifeValue = buffExecutingInfo_old.LifeValue;
				AISimulationBuffInfo buffInfo = (num ? new AISimulationBuffInfo(attackValue, lifeValue, attackValue, lifeValue) : new AISimulationBuffInfo(0, 0, attackValue, lifeValue));
				collection.Add(tagHash, targets, buffInfo);
			}
		}
	}

	public override void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		AIVirtualCard actor = situation.Actor;
		if (!CheckTriggerLegal(actor, tagOwner, playPtn, situation))
		{
			return;
		}
		List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		bool flag = actor.IsSameCardIncluded(targets);
		bool flag2 = situation.AttackTarget.IsSameCardIncluded(targets);
		if (flag || flag2)
		{
			if (flag)
			{
				int attack = (int)_attack.EvalArg(tagOwner, playPtn, field, situation);
				int life = (int)_life.EvalArg(tagOwner, playPtn, field, situation);
				information.AddAttackerWhenAttackBuff(attack, life);
			}
			if (flag2)
			{
				int value = (int)_life.EvalArg(tagOwner, playPtn, field, situation);
				information.AddTargetLifeBuff(value);
			}
		}
	}
}
