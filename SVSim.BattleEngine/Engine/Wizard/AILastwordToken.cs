using System.Collections.Generic;

namespace Wizard;

public class AILastwordToken : AIFiltersArgument
{
	private AIPolishConvertedExpression _tokenCount;

	private AIScriptTokenArgType _tokenSide;

	private int _realTokenCountIndexOffset;

	protected override int NON_FILTER_FIRST_OFFSET => _realTokenCountIndexOffset;

	public AILastwordToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		bool flag = InitializeTokenSide();
		_realTokenCountIndexOffset = 2 - ((!flag) ? 1 : 0);
		InitializeFilter();
		_tokenCount = _exprList[_exprList.Count - _realTokenCountIndexOffset];
	}

	private bool InitializeTokenSide()
	{
		if (AIPlayTagInitializingUtility.TryCreateTokenSideType(_exprList[_exprList.Count - 1], out var sideType))
		{
			_tokenSide = sideType;
			return true;
		}
		_tokenSide = AIScriptTokenArgType.ALLY;
		return false;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		int num = (int)_tokenCount.EvalArg(tagOwner, playPtn, field);
		if (num > 0)
		{
			AITokenIdCollection bothSideTokenIdListFromFilter = AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targetsFromField, base.Filters, AITokenType.Default, _tokenSide, AIScriptTokenArgType.ALL_SELECT, num, playPtn, situation);
			if (bothSideTokenIdListFromFilter != null && bothSideTokenIdListFromFilter.HasToken)
			{
				List<AIPlayTag> condList = null;
				bothSideTokenIdListFromFilter.SummonAllTokenToField(field, tagOwner, situation, condList);
			}
		}
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public AITokenIdCollection GetBothSideTokenIds(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, null, isBlockDead: false);
		int num = (int)_tokenCount.EvalArg(tagOwner, playPtn, field);
		if (num <= 0)
		{
			return null;
		}
		return AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targetsFromField, base.Filters, AITokenType.Default, _tokenSide, AIScriptTokenArgType.ALL_SELECT, num, playPtn, null);
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, _tokenSide, idList, AITokenType.Default);
	}
}
