using System.Collections.Generic;

namespace Wizard;

public class AIBreakSetLeaderMaxLife : AIFiltersArgument
{
	private AIScriptTokenArgType _side;

	private AIPolishConvertedExpression _life;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIBreakSetLeaderMaxLife(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_side = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
		_life = _exprList[_exprList.Count - 1];
	}

	protected override void RunMethod(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int maxLife = (int)_life.EvalArg(tagOwner, playPtn, field, situation);
		AISetStatusSimulationUtility.SetLeaderMaxLife(tagOwner, maxLife, _side, field, situation);
	}
}
