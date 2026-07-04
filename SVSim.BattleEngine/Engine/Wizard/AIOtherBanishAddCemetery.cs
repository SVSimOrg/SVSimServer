using System.Collections.Generic;

namespace Wizard;

public class AIOtherBanishAddCemetery : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _addCount;

	private readonly int COUNT_ARG_INDEX = 1;

	public AIOtherBanishAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_addCount = _exprList[_exprList.Count - COUNT_ARG_INDEX];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int count = (int)_addCount.EvalArg(tagOwner, playPtn, field, situation);
		field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
	}
}
