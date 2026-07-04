using System.Collections.Generic;

namespace Wizard;

public class AIEarthRite : AIScriptArgumentExpressions
{
	private List<AIPolishConvertedExpression> _earthRiteCountList;

	private static readonly int TIMING_ARG_OFFSET = 1;

	public AIScriptTokenArgType Timing { get; private set; }

	public AIEarthRite(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		int num = _exprList.Count - TIMING_ARG_OFFSET;
		_earthRiteCountList = new List<AIPolishConvertedExpression>();
		for (int i = 0; i < num; i++)
		{
			_earthRiteCountList.Add(_exprList[i]);
		}
		Timing = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[num]);
	}

	public int GetEarthRiteCount(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int currentStackCount)
	{
		int num = 0;
		for (int i = 0; i < _earthRiteCountList.Count; i++)
		{
			AIPolishConvertedExpression aIPolishConvertedExpression = _earthRiteCountList[i];
			if (aIPolishConvertedExpression.IsCertainArgumentTypeExpress(AIScriptTokenArgType.ALL))
			{
				return currentStackCount;
			}
			int num2 = (int)aIPolishConvertedExpression.EvalArg(owner, playPtn, field, situation);
			if (num2 <= currentStackCount && num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}
}
