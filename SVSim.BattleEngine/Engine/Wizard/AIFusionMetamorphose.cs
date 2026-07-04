using System.Collections.Generic;

namespace Wizard;

public class AIFusionMetamorphose : AIScriptArgumentExpressions
{
	private int _targetId;

	public AIFusionMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count <= 0 || _exprList.Count > 1)
		{
			_targetId = -1;
			AIConsoleUtility.LogError($"AIFusionMetamorphose(): Arg count is out of range. Please check the value. count:{_exprList.Count}");
		}
		else
		{
			_targetId = _exprList[0].EvalID();
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation == null || situation.ActionType != AIOperationType.FUSION)
		{
			AIConsoleUtility.LogError("AIFusionMetamorphose.Execute(): Situation is not FUSION action type.");
		}
		else if (_targetId > 0)
		{
			AIMetamorphoseSimulationUtility.MetamorphoseHandOnVirtualField(situation.Actor, _targetId, tagOwner, field);
		}
	}
}
