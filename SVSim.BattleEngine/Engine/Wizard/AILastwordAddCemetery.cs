using System.Collections.Generic;

namespace Wizard;

public class AILastwordAddCemetery : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _addCount;

	private readonly int COUNT_ARG_INDEX;

	public AILastwordAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_addCount = _exprList[COUNT_ARG_INDEX];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int count = (int)_addCount.EvalArg(tagOwner, playPtn, field, situation);
		field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
	}
}
