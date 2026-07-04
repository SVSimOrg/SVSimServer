using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayRecoverPp : AIWhenPlayTagArgument
{
	private readonly int NUM_ARG_OFFSET = 1;

	private AIPolishConvertedExpression _numberArgument;

	public AIWhenPlayRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_numberArgument = _exprList[_exprList.Count - NUM_ARG_OFFSET];
	}

	protected override void InitializeFilter()
	{
		base.Filters = null;
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.NONE;
	}

	public int GetPlayRecoverPp(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		return (int)_numberArgument.EvalArg(tagOwner, playPtn, field, situation);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY || situation.Actor == null)
		{
			return;
		}
		int playRecoverPp = GetPlayRecoverPp(tagOwner, playPtn, field, situation);
		if (playRecoverPp >= 0)
		{
			if (tagOwner.IsAlly)
			{
				field.AllyPp += playRecoverPp;
			}
			else
			{
				field.EnemyPp += playRecoverPp;
			}
		}
	}
}
