using System.Collections.Generic;

namespace Wizard;

public class AIEvoRecoverPp : AIEvoTagArgument
{
	private AIPolishConvertedExpression _recoverValue;

	private int RECOVER_VALUE_INDEX_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => -1;

	public AIEvoRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		if (_exprList == null || _exprList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIEvoRecoverPp error!! _exprList is null or Count==0");
		}
		else
		{
			_recoverValue = _exprList[_exprList.Count - RECOVER_VALUE_INDEX_OFFSET];
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int amount = (int)GetRecoverValue(tagOwner, field, playPtn, situation);
		field.RecoverPp(amount);
	}

	private float GetRecoverValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_recoverValue == null)
		{
			return 0f;
		}
		return _recoverValue.EvalArg(tagOwner, playPtn, field, situation);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}
}
