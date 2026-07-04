using System.Collections.Generic;

namespace Wizard;

public class AILastwordAddDeck : AIScriptArgumentExpressions
{
	private AIScriptIDToken _cardIdToken;

	private AIPolishConvertedExpression _addCountExpression;

	public AILastwordAddDeck(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_cardIdToken = _exprList[0].TokenList[0] as AIScriptIDToken;
		_addCountExpression = _exprList[1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		int tokenCount = (int)_addCountExpression.EvalArg(tagOwner, playPtn, field, situation);
		field.AddDeckCard(_cardIdToken.ID, tokenCount, tagOwner, playPtn, situation);
	}
}
