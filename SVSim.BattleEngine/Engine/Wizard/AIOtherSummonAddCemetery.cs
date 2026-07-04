using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonAddCemetery : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _addCemeteryCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherSummonAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_addCemeteryCount = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int count = (int)_addCemeteryCount.EvalArg(tagOwner, playPtn, field, situation);
		field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetCandidateRange(field);
	}
}
