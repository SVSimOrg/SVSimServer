using System.Collections.Generic;

namespace Wizard;

public class AIEvolvedAttackableCount : AIScriptArgumentExpressions
{
	private readonly int COUNT_ARG_INDEX;

	public AIPolishConvertedExpression Count { get; private set; }

	public AIEvolvedAttackableCount(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Count = _exprList[COUNT_ARG_INDEX];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		int count = (int)Count.EvalArg(tagOwner, playPtn, field, situation);
		if (tagOwner.IsEvolution)
		{
			tagOwner.GiveAttackableCount(count);
		}
	}
}
