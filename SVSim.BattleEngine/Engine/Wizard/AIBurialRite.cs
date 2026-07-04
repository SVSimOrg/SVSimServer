using System.Collections.Generic;

namespace Wizard;

public class AIBurialRite : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _value;

	private readonly int VALUE_INDEX;

	private readonly int TIMING_INDEX = 1;

	public AIScriptTokenArgType Timing { get; private set; }

	public AIBurialRite(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList != null && _exprList.Count >= TIMING_INDEX + 1)
		{
			_value = _exprList[VALUE_INDEX];
			Timing = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[TIMING_INDEX]);
			if (!CheckValidTimingType(Timing))
			{
				Timing = AIScriptTokenArgType.NONE;
			}
		}
	}

	public static bool CheckValidTimingType(AIScriptTokenArgType type)
	{
		if (type == AIScriptTokenArgType.WHEN_PLAY || type == AIScriptTokenArgType.WHEN_EVO)
		{
			return true;
		}
		return false;
	}

	public int GetBurialRiteCount(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, List<int> playPtn)
	{
		return (int)_value.EvalArg(tagOwner, playPtn, field, situation);
	}
}
