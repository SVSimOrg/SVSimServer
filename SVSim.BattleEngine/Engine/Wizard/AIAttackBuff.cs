using System.Collections.Generic;

namespace Wizard;

public class AIAttackBuff : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _attack;

	private AIPolishConvertedExpression _life;

	private AIScriptTokenArgType _tempOrPerm;

	private readonly int TEMP_OR_PERM_ARG_OFFSET = 1;

	private readonly int LIFE_ARG_OFFSET = 2;

	private readonly int ATTACK_ARG_OFFSET = 3;

	protected override int SELECT_TYPE_OFFSET => 4;

	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIAttackBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count > SELECT_TYPE_OFFSET)
		{
			_attack = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
			_life = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
			_tempOrPerm = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - TEMP_OR_PERM_ARG_OFFSET], AIBuffEvaluationUtility.LEGAL_TEMP_OR_PERM_ARGUMENTS);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attack, _life);
			bool isTemp = _tempOrPerm == AIScriptTokenArgType.TEMP;
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, isTemp, playPtn, situation);
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !_attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	public void RegisterBuffInfo(AISimulationBuffInfoCollection collection, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, ulong tagHash)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attack, _life);
			if (!buffExecutingInfo_old.IsMultiplyAttack && !buffExecutingInfo_old.IsMultiplyLife)
			{
				bool num = _tempOrPerm == AIScriptTokenArgType.TEMP;
				int attackValue = buffExecutingInfo_old.AttackValue;
				int lifeValue = buffExecutingInfo_old.LifeValue;
				AISimulationBuffInfo buffInfo = (num ? new AISimulationBuffInfo(attackValue, lifeValue, attackValue, lifeValue) : new AISimulationBuffInfo(0, 0, attackValue, lifeValue));
				collection.Add(tagHash, targetsFromField, buffInfo);
			}
		}
	}

	public override void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		bool flag = situation.Actor.IsSameCardIncluded(targetsFromField);
		bool flag2 = situation.AttackTarget.IsSameCardIncluded(targetsFromField);
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
