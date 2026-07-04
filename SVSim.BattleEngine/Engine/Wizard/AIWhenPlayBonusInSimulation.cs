using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayBonusInSimulation : AIWhenPlayTagArgument
{
	public AIWhenPlayBonusInSimulation(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.SelectType = AIScriptTokenArgType.NONE;
		base.Filters = null;
		InitExprList(text);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		field.SimulationExtraBonus += EvalArg(0, tagOwner, playPtn, field, situation);
	}
}
