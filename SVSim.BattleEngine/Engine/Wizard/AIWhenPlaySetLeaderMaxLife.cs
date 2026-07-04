using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySetLeaderMaxLife : AIWhenPlayTagArgument
{
	private AIScriptTokenArgType _side;

	private AIPolishConvertedExpression _life;

	public AIWhenPlaySetLeaderMaxLife(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_side = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
		_life = _exprList[_exprList.Count - 1];
	}

	protected override void InitializeFilter()
	{
		base.Filters = null;
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.NONE;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int maxLife = (int)_life.EvalArg(tagOwner, playPtn, field, situation);
		AISetStatusSimulationUtility.SetLeaderMaxLife(tagOwner, maxLife, _side, field, situation);
	}
}
