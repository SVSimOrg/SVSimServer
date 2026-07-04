using System.Collections.Generic;

namespace Wizard;

public class AIAttackBreakRecoverPp : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _recoverValue;

	public AIAttackBreakRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList != null && _exprList.Count > 0)
		{
			_recoverValue = _exprList[0];
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int amount = (int)_recoverValue.EvalArg(tagOwner, playPtn, field, situation);
		field.RecoverPp(amount);
	}
}
