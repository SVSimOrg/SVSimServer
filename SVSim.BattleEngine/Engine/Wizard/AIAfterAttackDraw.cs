using System.Collections.Generic;

namespace Wizard;

public class AIAfterAttackDraw : AIFiltersArgument
{
	private AIPolishConvertedExpression _drawCountArg;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIAfterAttackDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_drawCountArg = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation != null && situation.Actor != null && AIFilteringUtility.CheckMatchTargetFiltering(situation.Actor, null, base.Filters, playPtn, tagOwner, situation))
		{
			int drawCount = (int)_drawCountArg.EvalArg(tagOwner, playPtn, field, situation);
			field.DrawCard(tagOwner.IsAlly, drawCount, playPtn, situation);
		}
	}
}
