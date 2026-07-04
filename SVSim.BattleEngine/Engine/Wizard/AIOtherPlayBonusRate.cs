using System.Collections.Generic;

namespace Wizard;

public class AIOtherPlayBonusRate : AIFiltersArgument
{
	private AIPolishConvertedExpression _valueArg;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherPlayBonusRate(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_valueArg = _exprList[_exprList.Count - NON_FILTER_FIRST_OFFSET];
	}

	public float GetBonusRate(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		_ = selfField.ParamQuery;
		if (_valueArg == null || !AIFilteringUtility.CheckMatchTargetFiltering(targetCard, null, base.Filters, playPtn, tagOwner, situation))
		{
			return 1f;
		}
		return _valueArg.EvalArg(tagOwner, playPtn, selfField, situation);
	}
}
