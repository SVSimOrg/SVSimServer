using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayReanimate : AIWhenPlayTokenArgumentBase
{
	private AIPolishConvertedExpression _costArgument;

	protected override int SELECT_COUNT_OFFSET => -1;

	protected override int SELECT_TYPE_OFFSET => -1;

	public AIWhenPlayReanimate(string text)
		: base(text, AITokenType.Reanimate)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeSide();
		_costArgument = _exprList[_exprList.Count - (2 - _ommittedIndexOffset)];
		_realNonFilterFirstOffset = 2 - _ommittedIndexOffset;
		InitSelectType();
		InitializeFilter();
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.RANDOM_SELECT;
	}

	protected override void InitializeFilter()
	{
		List<AIPolishConvertedExpression> filterExpressionList = AIPlayTagInitializingUtility.GetFilterExpressionList(_exprList, NON_FILTER_FIRST_OFFSET);
		base.Filters = new List<AIScriptTokenBase>();
		if (filterExpressionList != null)
		{
			base.Filters = GetFilters(filterExpressionList);
		}
		_candidateRangeInfo = CreateCandidateRangeInfo();
	}

	protected override AITokenIdHolderCandidateRangeInformation CreateCandidateRangeInfo()
	{
		return AITokenIdHolderCandidateRangeInformation.CreateReanimateRangeInformation(SideType);
	}

	protected override AITokenIdCollection GetBothSideTokenIdCollectionFromTag(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		bool isTokenAlly;
		int reanimateTokenId = AIReanimateSimulationUtility.GetReanimateTokenId(tagOwner, field, playPtn, situation, base.Filters, _costArgument, SideType, out isTokenAlly);
		if (reanimateTokenId == -1)
		{
			return null;
		}
		return AISummonTokenUtility.CreateTokenIdCollectionForReanimate(tagOwner, reanimateTokenId, isTokenAlly);
	}

	public override List<AITokenInformation> GetAllyTokenIdList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation)?.AllyTokenIdList;
	}
}
