using System.Collections.Generic;

namespace Wizard;

public class AIBreakRecoverPp : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _recoverPpValue;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIBreakRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_recoverPpValue = _exprList[_exprList.Count - 1];
	}

	private int GetRecoverValue(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_recoverPpValue == null)
		{
			return 0;
		}
		return (int)_recoverPpValue.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		field.RecoverPp(GetRecoverValue(tagOwner, playPtn, situation));
	}
}
