using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonDraw : AITriggerAndTargetFiltersTagBase
{

	public AIPolishConvertedExpression DrawCount { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherSummonDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		DrawCount = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (DrawCount != null)
		{
			int drawCount = (int)DrawCount.EvalArg(tagOwner, playPtn, field, situation);
			field.DrawCard(tagOwner.IsAlly, drawCount, playPtn, situation);
		}
	}
}
