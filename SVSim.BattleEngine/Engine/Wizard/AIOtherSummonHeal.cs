using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonHeal : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Heal { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherSummonHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		Heal = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int heal = (int)Heal.EvalArg(tagOwner, playPtn, field);
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
