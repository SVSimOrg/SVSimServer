using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonDamageCut : AIOtherSummonBarrierBase
{
	private AIPolishConvertedExpression _cutAmount;

	protected override int _defaultDamageTypeOffset => 3;

	protected override int _stopTimingOffset => 2;

	public AIOtherSummonDamageCut(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_cutAmount = _exprList[_exprList.Count - 1];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int cutAmount = (int)_cutAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageCutToAll(targets, tagOwner, field, _damageType, _stopTiming, cutAmount);
	}
}
