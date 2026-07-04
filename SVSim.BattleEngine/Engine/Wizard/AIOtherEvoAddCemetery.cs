using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoAddCemetery : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _addCemeteryCount;

	protected override int SELECT_TYPE_ARG_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherEvoAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_addCemeteryCount = _exprList[_exprList.Count - 1];
	}

	protected override void InitializeSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int count = (int)_addCemeteryCount.EvalArg(tagOwner, playPtn, field, situation);
		field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
	}
}
