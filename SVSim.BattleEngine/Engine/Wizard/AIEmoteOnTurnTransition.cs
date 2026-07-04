namespace Wizard;

public class AIEmoteOnTurnTransition : AIScriptArgumentExpressions
{
	private readonly int SIDE_INDEX;

	private readonly int EMOTE_ID_INDEX = 1;

	public AIScriptTokenArgType Side { get; private set; }

	public int EmoteId { get; private set; }

	public AIEmoteOnTurnTransition(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count <= EMOTE_ID_INDEX)
		{
			AIConsoleUtility.LogError($"AIEmoteOnTurnTransition Argument Error!! Arg count is not enough [count:{_exprList.Count}]");
			return;
		}
		AIPolishConvertedExpression arg = _exprList[SIDE_INDEX];
		if (IsSideTokenArgType(arg, out var dstTokenARgType))
		{
			Side = dstTokenARgType;
		}
		else
		{
			Side = AIScriptTokenArgType.ALLY;
			AIConsoleUtility.LogError("AIEmoteOnTurnTransition Side Expression Error!! SideType =" + dstTokenARgType);
		}
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[EMOTE_ID_INDEX];
		if (aIPolishConvertedExpression.TokenList != null && aIPolishConvertedExpression.TokenList.Count > 0)
		{
			AIScriptTokenBase aIScriptTokenBase = aIPolishConvertedExpression.TokenList[0];
			EmoteId = (int)aIScriptTokenBase.Value;
		}
		else
		{
			EmoteId = -1;
		}
	}

	public int GetEmoteIdIfSideIsCorrect(bool isOwnerTurn)
	{
		if (Side == AIScriptTokenArgType.BOTH || (Side == AIScriptTokenArgType.ALLY && isOwnerTurn) || (Side == AIScriptTokenArgType.OPPONENT && !isOwnerTurn))
		{
			return EmoteId;
		}
		return -1;
	}
}
