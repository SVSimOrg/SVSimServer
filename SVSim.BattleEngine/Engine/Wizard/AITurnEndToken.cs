using System.Collections.Generic;

namespace Wizard;

public class AITurnEndToken : AIFiltersArgument, IAITurnEndArgument
{
	private AIScriptTokenArgType _tokenSide;

	private AIPolishConvertedExpression _tokenCount;

	private int _realTokenCountIndexOffset;

	public bool IsAllyTurn { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => _realTokenCountIndexOffset;

	public AITurnEndToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		int num = 0;
		if (AIPlayTagInitializingUtility.TryCreateTokenSideType(_exprList[_exprList.Count - 1], out var sideType))
		{
			_tokenSide = sideType;
		}
		else
		{
			_tokenSide = AIScriptTokenArgType.ALLY;
			num = 1;
		}
		int num2 = 2 - num;
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - num2);
		_realTokenCountIndexOffset = 3 - num;
		_tokenCount = _exprList[_exprList.Count - _realTokenCountIndexOffset];
		InitializeFilter();
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AISummonTokenUtility.ExecuteSummonToken(GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false), base.Filters, _tokenCount, _tokenSide, tagOwner, field, playPtn, situation);
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, _tokenSide, idList, AITokenType.Default);
	}
}
