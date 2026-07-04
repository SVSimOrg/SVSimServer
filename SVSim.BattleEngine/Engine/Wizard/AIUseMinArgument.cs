namespace Wizard;

public class AIUseMinArgument : AIScriptArgumentExpressions
{
	public bool IsUseMin { get; private set; }

	protected virtual int USE_MIN_INDEX_OFFSET => 1;

	public AIUseMinArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[_exprList.Count - USE_MIN_INDEX_OFFSET];
		if (aIPolishConvertedExpression.TokenList[0] is AIScriptArgumentToken && aIPolishConvertedExpression.TokenList.Count < 2)
		{
			if ((aIPolishConvertedExpression.TokenList[0] as AIScriptArgumentToken).ArgumentType == AIScriptTokenArgType.USE_MIN)
			{
				IsUseMin = true;
			}
			else
			{
				IsUseMin = false;
			}
		}
	}
}
