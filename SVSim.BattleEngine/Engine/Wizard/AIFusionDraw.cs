using System.Collections.Generic;

namespace Wizard;

public class AIFusionDraw : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _drawCount;

	public AIFusionDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_drawCount = _exprList[0];
	}

	public int GetDrawCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_drawCount == null)
		{
			AIConsoleUtility.LogError("AIFusionDraw.GetDrawCount() error!! _drawCount is null");
			return 0;
		}
		return (int)_drawCount.EvalArg(tagOwner, playPtn, field, situation);
	}
}
