using System.Collections.Generic;

namespace Wizard;

public class AIOtherPlayBonus : AIUseMinArgument
{
	private AIPolishConvertedExpression valueArg;

	public List<AIScriptTokenBase> Filters { get; private set; }

	public AIOtherPlayBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count >= 2)
		{
			int num = (base.IsUseMin ? (USE_MIN_INDEX_OFFSET + 1) : USE_MIN_INDEX_OFFSET);
			Filters = GetFilters(_exprList.GetRange(0, _exprList.Count - num));
			valueArg = _exprList[_exprList.Count - num];
		}
	}

	public float GetEvaluateValue(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		return valueArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}
}
