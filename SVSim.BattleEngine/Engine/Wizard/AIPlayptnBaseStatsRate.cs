using System.Collections.Generic;

namespace Wizard;

public class AIPlayptnBaseStatsRate : AIFiltersArgument
{
	private AIPolishConvertedExpression _rate;

	private int RATE_INDX_OFFSET = 1;

	protected override int NON_FILTER_FIRST_OFFSET => RATE_INDX_OFFSET;

	public AIPlayptnBaseStatsRate(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_rate = _exprList[_exprList.Count - RATE_INDX_OFFSET];
	}

	public int GetRateValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn)
	{
		return (int)_rate.EvalArg(tagOwner, playPtn, field);
	}
}
