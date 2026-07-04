using System.Collections.Generic;

namespace Wizard;

public class AITurnStartSubtractCountdown : AITurnStartTagArgument
{
	private readonly int COUNT_ARG_OFFSET = 2;

	public AIPolishConvertedExpression CountDown { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnStartSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		CountDown = _exprList[_exprList.Count - COUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int value = (int)CountDown.EvalArg(tagOwner, playPtn, field);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISubtractCountdownSimulationUtility.SubtractCountdownAll(targetsFromField, value, situation);
			}
		}
	}
}
