using System.Collections.Generic;

namespace Wizard;

public class AIFilteringActivateCountArgument : AIActivateCountTagArgument
{

	public List<AIScriptTokenBase> Filters { get; private set; }

	public AIFilteringActivateCountArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Filters = GetFilters(_exprList.GetRange(0, _exprList.Count - 4));
		if (_exprList[_exprList.Count - 4].TokenList[0] is AIScriptNumericToken aIScriptNumericToken)
		{
			base.TurnMaxActivateCount = (int)aIScriptNumericToken.Value;
		}
	}
}
