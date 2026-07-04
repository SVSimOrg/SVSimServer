using System.Collections.Generic;

namespace Wizard;

public class AILastwordDraw : AIScriptArgumentExpressions
{

	public AIPolishConvertedExpression DrawCount { get; private set; }

	public AILastwordDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		DrawCount = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (DrawCount != null)
		{
			int drawCount = (int)DrawCount.EvalArg(tagOwner, playPtn, field, situation);
			tagOwner.SelfField.DrawCard(tagOwner.IsAlly, drawCount, playPtn, situation);
		}
	}
}
