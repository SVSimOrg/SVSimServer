using System.Collections.Generic;

namespace Wizard;

public class AITurnStartDamageCut : AITurnStartBarrierBase
{
	private AIPolishConvertedExpression _cutAmount;

	protected override int _stopTimingOffset => 3;

	protected override int _defaultDamageTypeOffset => 4;

	public AITurnStartDamageCut(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_cutAmount = _exprList[_exprList.Count - 2];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int cutAmount = (int)_cutAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageCutToAll(targets, tagOwner, field, _damageType, _stopTiming, cutAmount);
	}
}
