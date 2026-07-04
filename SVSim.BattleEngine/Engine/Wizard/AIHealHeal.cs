using System.Collections.Generic;

namespace Wizard;

public class AIHealHeal : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _healValue;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIHealHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		_healValue = _exprList[_exprList.Count - 1];
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
			int heal = (int)_healValue.EvalArg(tagOwner, playPtn, field, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
			}
		}
	}
}
