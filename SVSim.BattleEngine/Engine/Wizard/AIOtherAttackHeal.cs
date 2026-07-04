using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackHeal : AIWhenAttackSelfAndOtherTagArgument
{
	private AIScriptTokenArgType _selectType;

	private AIPolishConvertedExpression _heal;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherAttackHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_heal = _exprList[_exprList.Count - 1];
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int heal = (int)_heal.EvalArg(tagOwner, playPtn, field, situation);
			if (_selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
			}
		}
	}
}
