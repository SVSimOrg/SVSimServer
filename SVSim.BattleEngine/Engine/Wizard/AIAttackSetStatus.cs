using System.Collections.Generic;

namespace Wizard;

public class AIAttackSetStatus : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _attackExpression;

	private AIPolishConvertedExpression _lifeExpression;

	private readonly int ATTACK_ARG_OFFSET = 2;

	private readonly int LIFE_ARG_OFFSET = 1;

	private bool _isSetAttack;

	private bool _isSetLife;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIAttackSetStatus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackExpression = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		_lifeExpression = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
		_isSetAttack = !AISetStatusSimulationUtility.IsNoneSetValue(_attackExpression);
		_isSetLife = !AISetStatusSimulationUtility.IsNoneSetValue(_lifeExpression);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			ExecuteSetStatusToAll(targetsFromField, tagOwner, field, playPtn, situation);
		}
	}

	private void ExecuteSetStatusToAll(List<AIVirtualCard> targetList, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int attack = (_isSetAttack ? ((int)_attackExpression.EvalArg(tagOwner, playPtn, field, situation)) : 0);
		int newLife = (_isSetLife ? ((int)_lifeExpression.EvalArg(tagOwner, playPtn, field, situation)) : 0);
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			if (_isSetAttack)
			{
				aIVirtualCard.SetAttack(attack);
			}
			if (_isSetLife)
			{
				aIVirtualCard.SetLife(newLife, situation);
			}
		}
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		if (!_isSetLife)
		{
			return false;
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(target, GetCandidateRange(field), base.Filters, playPtn, tagOwner, situation))
		{
			int num = (int)_lifeExpression.EvalArg(tagOwner, playPtn, field, situation);
			int num2 = target.Life - totalDamage;
			totalDamage = num2 - num;
			if (num <= 0)
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanKillAnyTarget(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, List<AIBarrierPseudoSimulationInfo> simBarrierList, int[] realDamageList)
	{
		if (!_isSetLife)
		{
			return false;
		}
		if (targetList == null || targetList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < simBarrierList.Count; i++)
		{
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = simBarrierList[i];
			AIVirtualCard owner = aIBarrierPseudoSimulationInfo.Owner;
			int totalDamage = realDamageList[i];
			if (CanKillTarget(tagOwner, owner, field, situation, playPtn, aIBarrierPseudoSimulationInfo, ref totalDamage))
			{
				return true;
			}
		}
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: true, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
