using System.Collections.Generic;

namespace Wizard;

public class AIDamagedBuff : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _atkBuff;

	private AIPolishConvertedExpression _lifeBuff;

	public AIDamagedBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count < 2)
		{
			AIConsoleUtility.LogError("AIDamagedBuff Argument Error!! Arg Count = " + _exprList.Count);
			return;
		}
		_atkBuff = _exprList[0];
		_lifeBuff = _exprList[1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _atkBuff, _lifeBuff);
		AIBuffSimulationUtility.BuffSingle_old(tagOwner, field, buffExecutingInfo_old, isTemp: false, playPtn, situation);
	}

	public int GetAttackBuff(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_atkBuff == null)
		{
			return 0;
		}
		if (_atkBuff.IsMultiplyMarked)
		{
			return 0;
		}
		return (int)_atkBuff.EvalArg(tagOwner, playPtn, field, situation);
	}

	public int GetLifeBuff(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_lifeBuff == null)
		{
			return 0;
		}
		if (_lifeBuff.IsMultiplyMarked)
		{
			return 0;
		}
		return (int)_lifeBuff.EvalArg(tagOwner, playPtn, field, situation);
	}
}
