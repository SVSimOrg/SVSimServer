using System.Collections.Generic;

namespace Wizard;

public class AIStack : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _defaultStackValue;

	public AIStack(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList == null || _exprList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIStack error!! exprList is null or Count == 0");
		}
		else
		{
			_defaultStackValue = _exprList[0];
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int stackValue = GetStackValue(tagOwner, field, playPtn, situation);
		AIWhiteRitualSimulationUtility.SimulateStack(tagOwner, field, situation, stackValue);
	}

	private int GetStackValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_defaultStackValue == null)
		{
			return 0;
		}
		return (int)_defaultStackValue.EvalArg(tagOwner, playPtn, field, situation);
	}
}
