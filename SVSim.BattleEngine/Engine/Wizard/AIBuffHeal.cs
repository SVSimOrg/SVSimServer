using System.Collections.Generic;

namespace Wizard;

public class AIBuffHeal : AITriggerAndTargetFiltersTagBase
{

	private AIPolishConvertedExpression _healArg;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIBuffHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		_healArg = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int heal = (int)_healArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AISkillSimulationUtility.HealSingle(targets[0], field, heal, playPtn, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
