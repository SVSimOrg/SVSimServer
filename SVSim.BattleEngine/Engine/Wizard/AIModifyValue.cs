using System.Collections.Generic;

namespace Wizard;

public class AIModifyValue : AIFiltersArgument
{
	private AIPolishConvertedExpression _modifyValueArg;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIModifyValue(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_ = _exprList.Count;
		_ = 1;
		_modifyValueArg = _exprList[_exprList.Count - 1];
	}

	public int GetModifiedValue(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation, int originalValue)
	{
		if (!AIFilteringUtility.CheckMatchTargetFiltering(targetCard, null, base.Filters, playPtn, tagOwner, null))
		{
			return originalValue;
		}
		return (int)_modifyValueArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}
}
