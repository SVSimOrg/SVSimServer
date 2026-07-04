using System.Collections.Generic;

namespace Wizard;

public class AINecromance : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression valueArg;

	public AIScriptTokenArgType Timing { get; private set; }

	public AINecromance(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count < 2)
		{
			return;
		}
		valueArg = _exprList[0];
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[1];
		if (aIPolishConvertedExpression.TokenList[0] is AIScriptArgumentToken)
		{
			AIScriptArgumentToken aIScriptArgumentToken = aIPolishConvertedExpression.TokenList[0] as AIScriptArgumentToken;
			if (CheckValidTimingType(aIScriptArgumentToken.ArgumentType))
			{
				Timing = aIScriptArgumentToken.ArgumentType;
			}
			else
			{
				Timing = AIScriptTokenArgType.NONE;
			}
		}
	}

	public int GetNecromanceValue(AIVirtualCard tagOwner, int currentCemetery, AIVirtualField field, AISituationInfo situation, List<int> playPtn)
	{
		if (valueArg == null)
		{
			return 0;
		}
		int num = (int)valueArg.EvalArg(tagOwner, playPtn, field, situation);
		if (num <= currentCemetery)
		{
			return num;
		}
		return 0;
	}

	public bool CheckValidTimingType(AIScriptTokenArgType type)
	{
		if ((uint)(type - 60) <= 2u || (uint)(type - 65) <= 2u)
		{
			return true;
		}
		return false;
	}
}
