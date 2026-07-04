using System.Collections.Generic;

namespace Wizard;

public class AITurnEndAddDeck : AIScriptArgumentExpressions, IAITurnEndArgument
{
	private AIPolishConvertedExpression _cardIdToken;

	private AIPolishConvertedExpression _addCountExpression;

	public bool IsAllyTurn { get; private set; }

	public AITurnEndAddDeck(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_cardIdToken = _exprList[0];
		_addCountExpression = _exprList[1];
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), 2);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		int tokenId = GetTokenId();
		int addCount = GetAddCount(tagOwner, field, playPtn, situation);
		field.AddDeckCard(tokenId, addCount, tagOwner, playPtn, situation);
	}

	public int GetTokenId()
	{
		return _cardIdToken.EvalID();
	}

	public int GetAddCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_addCountExpression == null)
		{
			return 0;
		}
		return (int)_addCountExpression.EvalArg(tagOwner, playPtn, field, situation);
	}
}
