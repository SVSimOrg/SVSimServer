using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoBonus : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression valueArg;

	public AIOtherEvoBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		valueArg = _exprList[0];
	}

	public float GetEvaluateValue(AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
	{
		return valueArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}
}
