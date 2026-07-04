using System.Collections.Generic;

namespace Wizard;

public class AILastwordReanimate : AIFiltersArgument
{
	private AIPolishConvertedExpression _costArgument;

	public AIScriptTokenArgType TokenSide;

	private int _ommittedIndexOffset;

	private int _realNonFilterFirstOffset;

	protected override int NON_FILTER_FIRST_OFFSET => _realNonFilterFirstOffset;

	public AILastwordReanimate(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeSide();
		_costArgument = _exprList[_exprList.Count - (2 - _ommittedIndexOffset)];
		_realNonFilterFirstOffset = 2 - _ommittedIndexOffset;
		InitializeFilter();
	}

	private void InitializeSide()
	{
		if (AIPlayTagInitializingUtility.TryCreateTokenSideType(_exprList[_exprList.Count - 1], out var sideType))
		{
			_ommittedIndexOffset = 0;
			TokenSide = sideType;
		}
		else
		{
			_ommittedIndexOffset = 1;
			TokenSide = AIScriptTokenArgType.ALLY;
		}
	}

	protected override void InitializeFilter()
	{
		base.Filters = new List<AIScriptTokenBase>();
		int num = _exprList.Count - NON_FILTER_FIRST_OFFSET;
		if (num > 0)
		{
			List<AIPolishConvertedExpression> range = _exprList.GetRange(0, num);
			base.Filters = GetFilters(range);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		bool isTokenAlly;
		int reanimateTokenId = AIReanimateSimulationUtility.GetReanimateTokenId(tagOwner, field, playPtn, situation, base.Filters, _costArgument, TokenSide, out isTokenAlly);
		if (reanimateTokenId != -1)
		{
			AISummonTokenUtility.CreateTokenIdCollectionForReanimate(tagOwner, reanimateTokenId, isTokenAlly).SummonAllTokenToField(field, tagOwner, situation);
		}
	}
}
