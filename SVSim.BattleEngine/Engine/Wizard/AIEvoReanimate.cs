using System.Collections.Generic;

namespace Wizard;

public class AIEvoReanimate : AIEvoTagArgument
{
	private AIPolishConvertedExpression _costArgument;

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => 0;

	public AIEvoReanimate(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_costArgument = _exprList[0];
		InitializeFilter();
	}

	protected override void InitializeFilter()
	{
		List<AIPolishConvertedExpression> filterExpressionList = AIPlayTagInitializingUtility.GetFilterExpressionList(_exprList, NON_FILTER_FIRST_OFFSET);
		base.Filters = new List<AIScriptTokenBase>();
		if (filterExpressionList != null)
		{
			base.Filters = GetFilters(filterExpressionList);
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		bool isTokenAlly;
		int reanimateTokenId = AIReanimateSimulationUtility.GetReanimateTokenId(tagOwner, field, playPtn, situation, base.Filters, _costArgument, AIScriptTokenArgType.ALLY, out isTokenAlly);
		if (reanimateTokenId != -1)
		{
			AISummonTokenUtility.CreateTokenIdCollectionForReanimate(tagOwner, reanimateTokenId, isTokenAlly).SummonAllTokenToField(field, tagOwner, situation);
		}
	}
}
