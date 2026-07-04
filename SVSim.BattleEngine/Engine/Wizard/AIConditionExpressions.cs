using System.Collections.Generic;

namespace Wizard;

public class AIConditionExpressions
{
	private AIPolishConvertedExpression _expr;

	public List<int> ReferringIds { get; private set; }

	public bool IsEmpty => _expr == null;

	public AIConditionExpressions(string text)
	{
		if (!(text == ""))
		{
			_expr = new AIPolishConvertedExpression(text);
			ReferringIds = _expr.GetReferringIDLists();
		}
	}

	public bool CheckCondition(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		if (_expr == null)
		{
			return true;
		}
		if (!_expr.CheckCondition(tagOwner, playPtn, field, situation))
		{
			return false;
		}
		return true;
	}

	public bool IsHoldingEVAL()
	{
		if (_expr != null)
		{
			return _expr.IsHoldingEVAL();
		}
		return false;
	}
}
