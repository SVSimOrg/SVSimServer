using System.Collections.Generic;

namespace Wizard;

public class AIDamagedBonus : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _bonus;

	public AIDamagedBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count <= 0)
		{
			AIConsoleUtility.LogError("DamagedBonus error!! _exprList.Count == 0");
		}
		else
		{
			_bonus = _exprList[0];
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		float num = CalcBonus(tagOwner, field, playPtn, situation);
		field.SimulationExtraBonus += ((field.AllyClass.IsAlly == tagOwner.IsAlly) ? num : (0f - num));
	}

	private float CalcBonus(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_bonus == null)
		{
			return 0f;
		}
		return _bonus.EvalArg(tagOwner, playPtn, field, situation);
	}
}
