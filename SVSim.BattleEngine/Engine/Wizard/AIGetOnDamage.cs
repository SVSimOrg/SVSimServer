using System.Collections.Generic;

namespace Wizard;

public class AIGetOnDamage : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _damageValueArg;

	public AIScriptTokenArgType SelectType;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIGetOnDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		_damageValueArg = _exprList[_exprList.Count - 1];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int damageValue = GetDamageValue(tagOwner, field, playPtn, situation);
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damageValue, situation);
		}
	}

	private int GetDamageValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageValueArg == null)
		{
			return 0;
		}
		return (int)_damageValueArg.EvalArg(tagOwner, playPtn, field, situation);
	}
}
