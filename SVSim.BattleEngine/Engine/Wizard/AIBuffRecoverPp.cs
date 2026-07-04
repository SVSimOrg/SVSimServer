using System.Collections.Generic;

namespace Wizard;

public class AIBuffRecoverPp : AITriggerAndTargetFiltersTagBase
{

	private AIPolishConvertedExpression _healArg;

	public AIBuffRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_healArg = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		field.RecoverPp(GetHealCount(tagOwner, playPtn, situation));
	}

	private int GetHealCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_healArg == null)
		{
			return 0;
		}
		return (int)_healArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}
}
