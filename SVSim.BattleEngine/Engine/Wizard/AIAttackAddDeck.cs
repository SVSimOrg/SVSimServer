using System.Collections.Generic;

namespace Wizard;

public class AIAttackAddDeck : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _count;

	private AIPolishConvertedExpression _id;

	public AIAttackAddDeck(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_id = _exprList[0];
		_count = _exprList[1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int tokenCount = (int)_count.EvalArg(tagOwner, playPtn, field, situation);
		int tokenId = _id.EvalID();
		field.AddDeckCard(tokenId, tokenCount, tagOwner, playPtn, situation);
	}
}
