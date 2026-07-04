using System.Collections.Generic;

namespace Wizard;

public class AIBuffDamage : AITriggerAndTargetFiltersTagBase
{

	private AIPolishConvertedExpression _damageArg;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIBuffDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		if (IsLegalSelectType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], out selectType))
		{
			SelectType = selectType;
		}
		_damageArg = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int damage = GetDamage(tagOwner, playPtn, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
			}
		}
	}

	private int GetDamage(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageArg == null)
		{
			return 0;
		}
		return (int)_damageArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
