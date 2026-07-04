using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoDraw : AIOtherEvoTagArgument
{

	public AIPolishConvertedExpression DrawCount { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	protected override int SELECT_TYPE_ARG_OFFSET => -1;

	public AIOtherEvoDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeFilters();
		DrawCount = _exprList[_exprList.Count - 1];
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
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

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = null;
	}
}
