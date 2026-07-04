namespace Wizard;

public class AIGiveSkill : AIFiltersArgument
{
	private AIScriptTokenArgType _skillType;

	public AIGiveSkill(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[0];
		if (aIPolishConvertedExpression.TokenList != null && aIPolishConvertedExpression.TokenList.Count > 0 && aIPolishConvertedExpression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken)
		{
			_skillType = aIScriptArgumentToken.ArgumentType;
		}
	}

	public AIScriptTokenArgType GetSkillType()
	{
		return _skillType;
	}
}
