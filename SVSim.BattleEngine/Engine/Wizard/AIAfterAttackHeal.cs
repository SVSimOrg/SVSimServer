using System.Collections.Generic;

namespace Wizard;

public class AIAfterAttackHeal : AITriggerAndTargetFiltersTagBase
{

	public AIPolishConvertedExpression HealAmount { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIAfterAttackHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		HealAmount = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int heal = (int)HealAmount.EvalArg(tagOwner, null, field, situation);
		if (targets != null && targets.Count > 0)
		{
			AISkillSimulationUtility.HealAll(targets, field, heal, field.BestPlayPtn, situation);
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
