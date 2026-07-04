using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoTokenDraw : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	protected override int SELECT_TYPE_ARG_OFFSET => -1;

	public AIOtherEvoTokenDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenCount = _exprList[_exprList.Count - 1];
	}

	protected override void InitializeSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetCandidateRange(field);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AISummonTokenUtility.ExecuteDrawToken(targets, base.TargetFilters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}
}
