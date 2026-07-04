using System.Collections.Generic;

namespace Wizard;

public class AIAfterClashHeal : AIFiltersAndSelectTypeArgument
{
	private readonly int HEAL_AMOUNT_ARG_OFFSET = 1;

	public AIPolishConvertedExpression HealAmount { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAfterClashHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		HealAmount = _exprList[_exprList.Count - HEAL_AMOUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int heal = (int)HealAmount.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targetsFromField, field, heal, playPtn, situation);
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
