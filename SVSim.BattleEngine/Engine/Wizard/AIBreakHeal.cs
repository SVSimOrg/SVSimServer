using System.Collections.Generic;

namespace Wizard;

public class AIBreakHeal : AITriggerAndTargetFiltersTagBase
{
	private readonly int HEAL_AMOUNT_OFFSET = 1;

	private readonly int SELECT_TYPE_OFFSET = 2;

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Heal { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIBreakHeal(string text)
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
		Heal = _exprList[_exprList.Count - HEAL_AMOUNT_OFFSET];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int heal = (int)Heal.EvalArg(tagOwner, playPtn, field, situation);
			AIScriptTokenArgType selectType = SelectType;
			if (selectType != AIScriptTokenArgType.RANDOM_SELECT && selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
